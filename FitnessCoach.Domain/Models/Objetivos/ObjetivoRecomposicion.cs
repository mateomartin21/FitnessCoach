namespace FitnessCoach.Domain.Models.Objetivos
{
    public class ObjetivoRecomposicion : ObjetivoFitness
    {
        public override string Nombre => "Recomposición y Fuerza (4 días)";
        public override double CalcularMultiplicadorCalorico() => 1.0;

        // Ganar músculo y perder grasa a la vez es el escenario más exigente en proteína:
        // se trabaja en el medio-alto del rango, con la grasa algo más baja para dejar
        // margen a los carbohidratos que sostienen el entrenamiento.
        public override double GramosProteinaPorKg => 2.0;
        public override double PorcentajeCaloriasDeGrasa => 0.22;
    }
}