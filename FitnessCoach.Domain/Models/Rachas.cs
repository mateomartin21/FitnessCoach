namespace FitnessCoach.Domain.Models
{
    /// <summary>Días consecutivos entrenando: la actual y el mejor registro histórico.</summary>
    public readonly record struct Rachas(int Actual, int MasLarga)
    {
        public static readonly Rachas Vacia = new(0, 0);
    }
}
