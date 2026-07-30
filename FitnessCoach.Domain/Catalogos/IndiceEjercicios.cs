using FitnessCoach.Domain.Models.Entrenamiento;

namespace FitnessCoach.Domain.Catalogos
{
    /// <summary>
    /// Foto inmutable del catálogo de ejercicios con sus accesos ya preparados. Es lógica
    /// pura: recibe la lista y arma los índices, sin saber de dónde vino ni quién la guarda
    /// en caché (de eso se encarga el decorador del adaptador SQL).
    ///
    /// Compartirla entre peticiones es seguro porque el catálogo es de solo lectura: se
    /// puebla por semilla al arrancar y nadie lo modifica en caliente.
    /// </summary>
    public sealed class IndiceEjercicios
    {
        /// <summary>Ordenados por nombre, igual que los devolvía el adaptador SQL.</summary>
        public IReadOnlyList<Ejercicio> Todos { get; }

        private readonly IReadOnlyDictionary<string, Ejercicio> _porSlug;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<Ejercicio>> _porGrupoMuscular;

        private IndiceEjercicios(
            IReadOnlyList<Ejercicio> todos,
            IReadOnlyDictionary<string, Ejercicio> porSlug,
            IReadOnlyDictionary<string, IReadOnlyList<Ejercicio>> porGrupoMuscular)
        {
            Todos = todos;
            _porSlug = porSlug;
            _porGrupoMuscular = porGrupoMuscular;
        }

        /// <summary>
        /// Los índices comparan sin distinguir mayúsculas para no cambiar el comportamiento
        /// que tenía la consulta SQL: la colación por defecto de SQL Server ya era
        /// insensible, y con un diccionario ordinal "Pecho" y "pecho" dejarían de coincidir.
        /// </summary>
        public static IndiceEjercicios Armar(IEnumerable<Ejercicio> ejercicios)
        {
            var ordenados = (ejercicios ?? Array.Empty<Ejercicio>())
                .OrderBy(e => e.Nombre)
                .ToList();

            var porSlug = ordenados
                .GroupBy(e => e.Slug ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var porGrupo = ordenados
                .GroupBy(e => e.GrupoMuscular ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key,
                              g => (IReadOnlyList<Ejercicio>)g.ToList(),
                              StringComparer.OrdinalIgnoreCase);

            return new IndiceEjercicios(ordenados, porSlug, porGrupo);
        }

        public Ejercicio? PorSlug(string? slug) =>
            slug is not null && _porSlug.TryGetValue(slug, out var ejercicio) ? ejercicio : null;

        public IReadOnlyList<Ejercicio> PorGrupoMuscular(string? grupoMuscular) =>
            grupoMuscular is not null && _porGrupoMuscular.TryGetValue(grupoMuscular, out var lista)
                ? lista
                : Array.Empty<Ejercicio>();
    }
}
