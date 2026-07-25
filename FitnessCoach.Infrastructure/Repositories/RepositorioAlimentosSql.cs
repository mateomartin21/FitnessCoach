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

        public Alimento? ObtenerPorSlug(string slug) =>
            _context.Alimentos.AsNoTracking().FirstOrDefault(a => a.Slug == slug);

        public IReadOnlyList<Alimento> ObtenerPorCategoria(string categoria) =>
            _context.Alimentos.AsNoTracking()
                .Where(a => a.Categoria == categoria)
                .OrderBy(a => a.Nombre)
                .ToList();

        public IReadOnlyList<Alimento> ObtenerPorGrupoIntercambio(string grupoIntercambio) =>
            _context.Alimentos.AsNoTracking()
                .Where(a => a.GrupoIntercambio == grupoIntercambio)
                .OrderBy(a => a.Nombre)
                .ToList();
    }
}
