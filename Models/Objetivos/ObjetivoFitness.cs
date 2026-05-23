namespace FitnessCoach.Models.Objetivos
{
    public abstract class ObjetivoFitness
    {
        public abstract string Nombre { get; }
        public abstract double CalcularMultiplicadorCalorico();
    }
}