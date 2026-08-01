using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCoach.Infrastructure.Repositories
{
    /// <summary>
    /// Adaptador SQL del catálogo de alimentos. Es de solo lectura: el catálogo se
    /// puebla por semilla, no desde la aplicación.
    /// </summary>
    public class RepositorioAlimentosSql : IRepositorioAlimentos
    {
        private readonly ApplicationDbContext _context;

        public RepositorioAlimentosSql(ApplicationDbContext context)
        {
            _context = context;
        }

        // AsNoTracking en todas: son datos de referencia que nadie modifica,
        // así que rastrearlos solo cuesta memoria.
        public IReadOnlyList<Alimento> ObtenerTodos() =>
            _context.Alimentos.AsNoTracking().OrderBy(a => a.Nombre).ToList();

        // PostgreSQL distingue mayusculas y SQL Server no (ADR-22): mismo criterio que en
        // el repositorio de ejercicios y que en el indice en memoria de la caché.
        public Alimento? ObtenerPorSlug(string slug) =>
            _context.Alimentos.AsNoTracking()
                .FirstOrDefault(a => a.Slug.ToLower() == slug.ToLower());

        public IReadOnlyList<Alimento> ObtenerPorCategoria(string categoria) =>
            _context.Alimentos.AsNoTracking()
                .Where(a => a.Categoria.ToLower() == categoria.ToLower())
                .OrderBy(a => a.Nombre)
                .ToList();

        public IReadOnlyList<Alimento> ObtenerPorGrupoIntercambio(string grupoIntercambio) =>
            _context.Alimentos.AsNoTracking()
                .Where(a => a.GrupoIntercambio.ToLower() == grupoIntercambio.ToLower())
                .OrderBy(a => a.Nombre)
                .ToList();
    }
}
