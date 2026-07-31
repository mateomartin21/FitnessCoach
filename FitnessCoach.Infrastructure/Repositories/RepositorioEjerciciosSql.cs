using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCoach.Infrastructure.Repositories
{
    /// <summary>
    /// Adaptador SQL del catálogo. Es de solo lectura: el catálogo se puebla por
    /// semilla, no desde la aplicación.
    /// </summary>
    public class RepositorioEjerciciosSql : IRepositorioEjercicios
    {
        private readonly ApplicationDbContext _context;

        public RepositorioEjerciciosSql(ApplicationDbContext context)
        {
            _context = context;
        }

        // AsNoTracking en todas: son datos de referencia que nadie modifica,
        // así que rastrearlos solo cuesta memoria.
        public IReadOnlyList<Ejercicio> ObtenerTodos() =>
            _context.Ejercicios.AsNoTracking().OrderBy(e => e.Nombre).ToList();

        public Ejercicio? ObtenerPorSlug(string slug) =>
            _context.Ejercicios.AsNoTracking().FirstOrDefault(e => e.Slug == slug);

        public IReadOnlyList<Ejercicio> ObtenerPorGrupoMuscular(string grupoMuscular) =>
            _context.Ejercicios.AsNoTracking()
                .Where(e => e.GrupoMuscular == grupoMuscular)
                .OrderBy(e => e.Nombre)
                .ToList();
    }
}
