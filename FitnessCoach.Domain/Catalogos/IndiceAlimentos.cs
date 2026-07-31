using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Catalogos
{
    /// <summary>Como <see cref="IndiceEjercicios"/>, con los cuatro accesos que pide el puerto.</summary>
    public sealed class IndiceAlimentos
    {
        public IReadOnlyList<Alimento> Todos { get; }

        private readonly IReadOnlyDictionary<string, Alimento> _porSlug;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<Alimento>> _porCategoria;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<Alimento>> _porGrupoIntercambio;

        private IndiceAlimentos(
            IReadOnlyList<Alimento> todos,
            IReadOnlyDictionary<string, Alimento> porSlug,
            IReadOnlyDictionary<string, IReadOnlyList<Alimento>> porCategoria,
            IReadOnlyDictionary<string, IReadOnlyList<Alimento>> porGrupoIntercambio)
        {
            Todos = todos;
            _porSlug = porSlug;
            _porCategoria = porCategoria;
            _porGrupoIntercambio = porGrupoIntercambio;
        }

        /// <summary>Sin distinguir mayúsculas, igual que la colación de SQL Server.</summary>
        public static IndiceAlimentos Armar(IEnumerable<Alimento> alimentos)
        {
            var ordenados = (alimentos ?? Array.Empty<Alimento>())
                .OrderBy(a => a.Nombre)
                .ToList();

            return new IndiceAlimentos(
                ordenados,
                ordenados.GroupBy(a => a.Slug ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                         .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase),
                Agrupar(ordenados, a => a.Categoria),
                Agrupar(ordenados, a => a.GrupoIntercambio));
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<Alimento>> Agrupar(
            List<Alimento> ordenados, Func<Alimento, string?> clave) =>
            ordenados.GroupBy(a => clave(a) ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                     .ToDictionary(g => g.Key,
                                   g => (IReadOnlyList<Alimento>)g.ToList(),
                                   StringComparer.OrdinalIgnoreCase);

        public Alimento? PorSlug(string? slug) =>
            slug is not null && _porSlug.TryGetValue(slug, out var alimento) ? alimento : null;

        public IReadOnlyList<Alimento> PorCategoria(string? categoria) => Buscar(_porCategoria, categoria);

        public IReadOnlyList<Alimento> PorGrupoIntercambio(string? grupo) => Buscar(_porGrupoIntercambio, grupo);

        private static IReadOnlyList<Alimento> Buscar(
            IReadOnlyDictionary<string, IReadOnlyList<Alimento>> indice, string? clave) =>
            clave is not null && indice.TryGetValue(clave, out var lista) ? lista : Array.Empty<Alimento>();
    }
}
