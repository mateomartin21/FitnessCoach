using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessCoach.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<UsuarioPerfil> UsuariosPerfil => Set<UsuarioPerfil>();

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
            
                entity.HasIndex(u => u.IdentityUserId)
                    .IsUnique()
                    .HasFilter("[IdentityUserId] IS NOT NULL");

            });
        }
    }
}