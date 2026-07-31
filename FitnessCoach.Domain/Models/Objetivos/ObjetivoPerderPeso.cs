namespace FitnessCoach.Domain.Models.Objetivos
{
    public class ObjetivoPerderPeso : ObjetivoFitness
    {
        public override string Nombre => "Pérdida de Grasa";
        public override double CalcularMultiplicadorCalorico() => 0.85;

        // En déficit es cuando más riesgo hay de perder músculo junto con la grasa:
        // se sube la proteína al tope del rango recomendado.
        public override double GramosProteinaPorKg => 2.2;
        public override double PorcentajeCaloriasDeGrasa => 0.25;
    }
}