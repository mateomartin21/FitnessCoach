namespace FitnessCoach.Domain.Catalogos
{
    /// <summary>
    /// Agrupa los doce valores de <c>Ejercicio.Equipo</c> del catálogo en opciones que
    /// una persona puede reconocer. Nadie sabe si tiene "lever" o "smith"; sí sabe si
    /// entrena en un gimnasio con máquinas.
    /// </summary>
    public static class EquipoEntrenamiento
    {
        public sealed record Grupo(string Valor, string Titulo, string Detalle, IReadOnlyList<string> Equipos);

        public static readonly IReadOnlyList<Grupo> Disponibles = new Grupo[]
        {
            new("peso-corporal", "Peso corporal", "Sin material: solo tu cuerpo", new[] { "bodyweight" }),
            new("mancuernas", "Mancuernas", "Un par de mancuernas o pesas rusas", new[] { "dumbbell", "kettlebell" }),
            new("bandas", "Bandas elásticas", "Bandas de resistencia", new[] { "band" }),
            new("barra", "Barra y discos", "Barra olímpica, barra Z y discos", new[] { "barbell", "ez-bar" }),
            new("poleas", "Poleas", "Torre de poleas o crossover", new[] { "cable" }),
            new("maquinas", "Máquinas", "Máquinas de gimnasio y multipower", new[] { "lever", "smith", "machine", "sled" })
        };

        /// <summary>Los valores crudos de <c>Equipo</c> que cubren los grupos elegidos.</summary>
        public static IReadOnlySet<string> EquiposDe(IEnumerable<string> grupos)
        {
            var elegidos = grupos.ToHashSet(StringComparer.OrdinalIgnoreCase);

            return Disponibles
                .Where(g => elegidos.Contains(g.Valor))
                .SelectMany(g => g.Equipos)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public static bool EsGrupoValido(string grupo) =>
            Disponibles.Any(g => string.Equals(g.Valor, grupo, StringComparison.OrdinalIgnoreCase));
    }
}
