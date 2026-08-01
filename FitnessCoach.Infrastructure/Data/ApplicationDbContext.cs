using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace FitnessCoach.Infrastructure.Data
{
    /// <summary>
    /// Implementa <see cref="IDataProtectionKeyContext"/> para que las claves con las que se
    /// firman las cookies vivan en la base y no en el disco del proceso. Sin esto, cada
    /// despliegue en un contenedor genera claves nuevas y **desloguea a todos los usuarios**.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

        private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

        // Al agregar una columna JSON no nula, EF genera `defaultValue: ""` y las filas que ya
        // existian quedan con la cadena vacia, que no es JSON valido. Leerla como coleccion
        // vacia evita que una migracion olvidada rompa todas esas filas (D-37).
        private static readonly ValueConverter<List<string>, string> ConversorListaTexto = new(
            lista => JsonSerializer.Serialize(lista, OpcionesJson),
            texto => string.IsNullOrWhiteSpace(texto)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(texto, OpcionesJson) ?? new List<string>());

        // Sin este comparador EF compara las listas por referencia y no detecta cambios
        // dentro de la coleccion.
        private static readonly ValueComparer<List<string>> ComparadorListaTexto = new(
            (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
            lista => lista.Aggregate(0, (acumulado, item) => HashCode.Combine(acumulado, item.GetHashCode())),
            lista => lista.ToList());

        // El diccionario se guarda igual que las listas, en una columna JSON. Se reconstruye
        // con el comparador sin distincion de mayusculas: los slugs vienen de SQL Server, que
        // no distingue, y perderlo haria que "Slug" y "slug" fueran claves distintas.
        private static Dictionary<string, string> NuevoMapa(IDictionary<string, string>? origen = null) =>
            origen is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(origen, StringComparer.OrdinalIgnoreCase);

        private static readonly ValueConverter<Dictionary<string, string>, string> ConversorMapaTexto = new(
            mapa => JsonSerializer.Serialize(mapa, OpcionesJson),
            // NuevoMapa(null) y no NuevoMapa(): un arbol de expresion no admite omitir opcionales.
            texto => string.IsNullOrWhiteSpace(texto)
                ? NuevoMapa(null)
                : NuevoMapa(JsonSerializer.Deserialize<Dictionary<string, string>>(texto, OpcionesJson)));

        private static readonly ValueComparer<Dictionary<string, string>> ComparadorMapaTexto = new(
            // Sin TryGetValue: un arbol de expresion no admite 'out var'.
            (a, b) => a!.Count == b!.Count && a.All(par => b.ContainsKey(par.Key) && b[par.Key] == par.Value),
            mapa => mapa.Aggregate(0, (acumulado, par) => HashCode.Combine(acumulado, par.Key.GetHashCode(), par.Value.GetHashCode())),
            mapa => NuevoMapa(mapa));

        public DbSet<UsuarioPerfil> UsuariosPerfil => Set<UsuarioPerfil>();
        public DbSet<Ejercicio> Ejercicios => Set<Ejercicio>();
        public DbSet<Alimento> Alimentos => Set<Alimento>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UsuarioPerfil>(entity =>
            {
                // Id IANA de zona horaria; el mas largo no pasa de 40 caracteres (D-25).
                entity.Property(u => u.ZonaHoraria).HasMaxLength(64);

                // ObjetivoActual es una clase abstracta sin datos propios (Strategy):
                // se guarda como el nombre del tipo y se reconstruye al leer.
                entity.Property(u => u.ObjetivoActual)
                    .HasConversion(
                        v => ObjetivoFitnessFactory.ObtenerNombreTipo(v),
                        v => ObjetivoFitnessFactory.CrearPorNombre(v))
                    .HasColumnName("ObjetivoActualTipo")
                    .HasMaxLength(100);

                // HistorialProgreso es una colección owned (no necesita su propia tabla con FK explícita en el dominio)
                entity.OwnsMany(u => u.HistorialProgreso, progreso =>
                {
                    progreso.WithOwner().HasForeignKey("UsuarioPerfilId");

                    // El Id ahora es una propiedad del dominio, no una shadow property:
                    // sin esto no se puede editar ni borrar un registro concreto (D-12).
                    progreso.HasKey(r => r.Id);

                    // SQL Server guarda datetime2 sin zona, asi que al leer vuelve con
                    // Kind = Unspecified. Marcarlo como UTC al materializar deja explicito
                    // que es un instante, para convertirlo a la zona del usuario (D-25).
                    progreso.Property(r => r.Fecha)
                        .HasConversion(
                            fecha => fecha,
                            fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

                    progreso.ToTable("RegistrosProgreso");
                });

                entity.OwnsMany(u => u.EntrenamientosCompletados, entrenamiento =>
                {
                    entrenamiento.WithOwner().HasForeignKey("UsuarioPerfilId");
                    entrenamiento.HasKey(e => e.Id);

                    // Mismo tratamiento que el historial de peso: sin esto la fecha vuelve
                    // como Unspecified y la conversion a local no haria nada.
                    entrenamiento.Property(e => e.Fecha)
                        .HasConversion(
                            fecha => fecha,
                            fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

                    entrenamiento.ToTable("EntrenamientosCompletados");
                });

                entity.OwnsMany(u => u.RecordsPersonales, record =>
                {
                    record.WithOwner().HasForeignKey("UsuarioPerfilId");
                    record.HasKey(r => r.Id);

                    record.Property(r => r.Fecha)
                        .HasConversion(
                            fecha => fecha,
                            fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

                    // Un solo record vigente por ejercicio y por usuario.
                    record.HasIndex("UsuarioPerfilId", nameof(RecordPersonal.EjercicioSlug)).IsUnique();

                    // Se calcula desde peso y repeticiones; no es columna.
                    record.Ignore(r => r.Volumen);

                    record.ToTable("RecordsPersonales");
                });

                entity.OwnsMany(u => u.Diario, comida =>
                {
                    comida.WithOwner().HasForeignKey("UsuarioPerfilId");
                    comida.HasKey(r => r.Id);

                    // Igual que el resto de las fechas: se lee como UTC para que la
                    // conversión a local de la vista funcione.
                    comida.Property(r => r.Fecha)
                        .HasConversion(
                            fecha => fecha,
                            fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

                    comida.Property(r => r.AlimentoSlug).HasMaxLength(100);
                    comida.Property(r => r.AlimentoNombre).HasMaxLength(120);

                    comida.ToTable("RegistrosComida");
                });

                // Preferencias es un objeto de valor sin identidad propia: vive en la
                // misma fila del perfil (OwnsOne), con las dos listas como columnas JSON.
                entity.OwnsOne(u => u.Preferencias, prefs =>
                {
                    prefs.Property(p => p.DietasSeguidas)
                        .HasConversion(ConversorListaTexto).Metadata.SetValueComparer(ComparadorListaTexto);
                    prefs.Property(p => p.AlimentosExcluidos)
                        .HasConversion(ConversorListaTexto).Metadata.SetValueComparer(ComparadorListaTexto);

                    prefs.Ignore(p => p.SinRestricciones);
                });
                // OwnsOne exige que la propiedad nunca sea null al materializar.
                entity.Navigation(u => u.Preferencias).IsRequired();

                entity.OwnsOne(u => u.PreferenciasEntrenamiento, prefs =>
                {
                    prefs.Property(p => p.EquipoDisponible)
                        .HasConversion(ConversorListaTexto).Metadata.SetValueComparer(ComparadorListaTexto);
                    prefs.Property(p => p.Sustituciones)
                        .HasConversion(ConversorMapaTexto).Metadata.SetValueComparer(ComparadorMapaTexto);

                    prefs.Ignore(p => p.SinRestricciones);
                });
                entity.Navigation(u => u.PreferenciasEntrenamiento).IsRequired();

                // Comillas dobles y no corchetes: es la sintaxis de PostgreSQL para
                // identificadores, y ademas distingue mayusculas (ADR-22).
                entity.HasIndex(u => u.IdentityUserId)
                    .IsUnique()
                    .HasFilter("\"IdentityUserId\" IS NOT NULL");

            });

            builder.Entity<Ejercicio>(entity =>
            {
                entity.ToTable("Ejercicios");
                entity.HasKey(e => e.Id);

                // El slug es como se identifica un ejercicio desde fuera de la base
                // (rutinas, URLs), asi que tiene que ser unico de verdad.
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();

                entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
                entity.Property(e => e.GrupoMuscular).HasMaxLength(60);
                entity.Property(e => e.ParteCuerpo).HasMaxLength(60);
                entity.Property(e => e.Equipo).HasMaxLength(60);
                entity.Property(e => e.Categoria).HasMaxLength(60);
                entity.Property(e => e.UrlGif).HasMaxLength(500);
                entity.Property(e => e.VideoYoutubeId).HasMaxLength(40);

                // Las listas de texto se guardan como JSON en una columna. Son datos de
                // solo lectura del catalogo: no se consultan por elemento ni se editan
                // sueltos, asi que una tabla aparte solo agregaria joins sin beneficio.
                entity.Property(e => e.MusculosSecundarios).HasConversion(ConversorListaTexto).Metadata
                    .SetValueComparer(ComparadorListaTexto);
                entity.Property(e => e.Instrucciones).HasConversion(ConversorListaTexto).Metadata
                    .SetValueComparer(ComparadorListaTexto);

                // TerminoBusquedaVideo se calcula desde el nombre; no es una columna.
                entity.Ignore(e => e.TerminoBusquedaVideo);
            });

            // Indices para los filtros que usan las estrategias al componer rutinas.
            builder.Entity<Ejercicio>().HasIndex(e => e.GrupoMuscular);
            builder.Entity<Ejercicio>().HasIndex(e => e.Equipo);

            builder.Entity<Alimento>(entity =>
            {
                entity.ToTable("Alimentos");
                entity.HasKey(a => a.Id);

                entity.HasIndex(a => a.Slug).IsUnique();
                entity.Property(a => a.Slug).HasMaxLength(100).IsRequired();

                entity.Property(a => a.Nombre).HasMaxLength(120).IsRequired();
                entity.Property(a => a.NombreIngles).HasMaxLength(250);
                entity.Property(a => a.Categoria).HasMaxLength(40);
                entity.Property(a => a.GrupoIntercambio).HasMaxLength(40);
                entity.Property(a => a.DescripcionPorcion).HasMaxLength(120);
                entity.Property(a => a.UrlImagen).HasMaxLength(500);
                entity.Property(a => a.AutorImagen).HasMaxLength(250);
                entity.Property(a => a.LicenciaImagen).HasMaxLength(100);

                entity.Property(a => a.EtiquetasDieta).HasConversion(ConversorListaTexto).Metadata
                    .SetValueComparer(ComparadorListaTexto);
                entity.Property(a => a.MomentosAptos).HasConversion(ConversorListaTexto).Metadata
                    .SetValueComparer(ComparadorListaTexto);

                // Las calorias y la atribucion se derivan de otras columnas.
                entity.Ignore(a => a.CaloriasPor100g);
                entity.Ignore(a => a.AtribucionImagen);

                // Indices para los filtros con que las estrategias arman las comidas.
                entity.HasIndex(a => a.Categoria);
                entity.HasIndex(a => a.GrupoIntercambio);
            });
        }
    }
}