namespace FitnessCoach.Domain.Patterns.Strategy
{
    /// <summary>
    /// Un renglón de la plantilla de un día: "3 ejercicios de pectorales, 4 series de 8-10".
    /// Describe qué se necesita, no cuáles: los concretos los elige el catálogo.
    /// </summary>
    /// <param name="GrupoMuscular">Grupo del catálogo (ej. "pectorals").</param>
    /// <param name="Cantidad">Cuántos ejercicios distintos de ese grupo.</param>
    /// <param name="Series">Series prescritas para cada uno.</param>
    /// <param name="Repeticiones">Repeticiones o consigna ("8-10", "Al fallo", "30 seg").</param>
    public readonly record struct BloqueEjercicios(
        string GrupoMuscular,
        int Cantidad,
        int Series,
        string Repeticiones);

    /// <summary>Plantilla de un día de entrenamiento, sin ejercicios concretos todavía.</summary>
    public sealed class PlantillaDia
    {
        public string NombreDia { get; init; } = string.Empty;
        public string Enfoque { get; init; } = string.Empty;
        public IReadOnlyList<BloqueEjercicios> Bloques { get; init; } = Array.Empty<BloqueEjercicios>();
    }
}
