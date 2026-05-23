namespace FitnessCoach.Models.Objetivos
{
    public class ObjetivoPerderPeso : ObjetivoFitness
    {
        public override string Nombre => "Pérdida de Grasa";
        public override double CalcularMultiplicadorCalorico() => 0.85;
    }
}
