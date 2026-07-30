using FitnessCoach.Domain.Catalogos;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Ports;
using Microsoft.Extensions.Caching.Memory;

namespace FitnessCoach.Infrastructure.Repositories
{
    /// <summary>
    /// Mismo decorador que el catálogo de ejercicios: menos filas, pero se piden más seguido
    /// (el selector del diario y las sustituciones por grupo de intercambio).
    /// </summary>
    public class RepositorioAlimentosEnCache : IRepositorioAlimentos
    {
        private const string Clave = "catalogo-alimentos";
        private static readonly TimeSpan Vigencia = TimeSpan.FromHours(12);

        private readonly IRepositorioAlimentos _origen;
        private readonly IMemoryCache _cache;

        public RepositorioAlimentosEnCache(IRepositorioAlimentos origen, IMemoryCache cache)
        {
            _origen = origen;
            _cache = cache;
        }

        public IReadOnlyList<Alimento> ObtenerTodos() => Indice().Todos;

        public Alimento? ObtenerPorSlug(string slug) => Indice().PorSlug(slug);

        public IReadOnlyList<Alimento> ObtenerPorCategoria(string categoria) =>
            Indice().PorCategoria(categoria);

        public IReadOnlyList<Alimento> ObtenerPorGrupoIntercambio(string grupoIntercambio) =>
            Indice().PorGrupoIntercambio(grupoIntercambio);

        private IndiceAlimentos Indice() =>
            _cache.GetOrCreate(Clave, entrada =>
            {
                entrada.AbsoluteExpirationRelativeToNow = Vigencia;
                return IndiceAlimentos.Armar(_origen.ObtenerTodos());
            })!;
    }
}
