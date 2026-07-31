namespace FitnessCoach.Domain.Models.Objetivos
{
    public class ObjetivoGanarMusculo : ObjetivoFitness
    {
        public override string Nombre => "Ganancia de Masa Muscular (Volumen)";
        // Para volumen, damos un superávit calórico del 15%
        public override double CalcularMultiplicadorCalorico() => 1.15;

        // Con superávit hay energía de sobra, así que alcanza el extremo bajo del rango:
        // el exceso de proteína no aporta más síntesis muscular y desplaza carbohidratos,
        // que son los que sostienen el entrenamiento de volumen.
        public override double GramosProteinaPorKg => 1.8;
        public override double PorcentajeCaloriasDeGrasa => 0.25;
    }
}