namespace FitnessCoach.Domain.Models.Objetivos
{
    public class ObjetivoRecomposicion : ObjetivoFitness
    {
        public override string Nombre => "Recomposición y Fuerza (4 días)";
        public override double CalcularMultiplicadorCalorico() => 1.0;
    }
}
