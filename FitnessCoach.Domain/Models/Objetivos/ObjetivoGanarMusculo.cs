namespace FitnessCoach.Domain.Models.Objetivos
{
    public class ObjetivoGanarMusculo : ObjetivoFitness
    {
        public override string Nombre => "Ganancia de Masa Muscular (Volumen)";
        // Para volumen, damos un superávit calórico del 15%
        public override double CalcularMultiplicadorCalorico() => 1.15;
    }
}
