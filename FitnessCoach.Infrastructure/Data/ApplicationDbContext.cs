using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace FitnessCoach.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

        private static readonly ValueConverter<List<string>, string> ConversorListaTexto = new(
            lista => JsonSerializer.Serialize(lista, OpcionesJson),
            texto => JsonSerializer.Deserialize<List<string>>(texto, OpcionesJson) ?? new List<string>());

        // Sin este comparador EF compara las listas por referencia y no detecta cambios
        // dentro de la coleccion.
        private static readonly ValueComparer<List<string>> ComparadorListaTexto = new(
            (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
            lista => lista.Aggregate(0, (acumulado, item) => HashCode.Combine(acumulado, item.GetHashCode())),
            lista => lista.ToList());

        public DbSet<UsuarioPerfil> UsuariosPerfil => Set<UsuarioPerfil>();
        public DbSet<Ejercicio> Ejercicios => Set<Ejercicio>();
        public DbSet<Alimento> Alimentos => Set<Alimento>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UsuarioPerfil>(entity =>
            {
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
                    // Kind = Unspecified y un ToLocalTime() posterior no convertiria nada.
                    // Marcarlo como UTC al materializar hace que la conversion a local funcione.
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
            
                entity.HasIndex(u => u.IdentityUserId)
                    .IsUnique()
                    .HasFilter("[IdentityUserId] IS NOT NULL");

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