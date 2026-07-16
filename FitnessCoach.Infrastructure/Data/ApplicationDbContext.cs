using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using Microsoft.EntityFrameworkCore;

namespace FitnessCoach.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
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
                    progreso.Property<int>("Id");
                    progreso.HasKey("Id");
                    progreso.ToTable("RegistrosProgreso");
                });
            });
        }
    }
}