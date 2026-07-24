namespace FitnessCoach.Domain.Models.Objetivos
{
    public abstract class ObjetivoFitness
    {
        public abstract string Nombre { get; }
        public abstract double CalcularMultiplicadorCalorico();

        /// <summary>
        /// Proteína diaria por kilo de peso corporal. Cada objetivo tiene el suyo:
        /// en déficit calórico se sube para preservar masa magra, en superávit alcanza menos
        /// porque hay energía de sobra para construir. Rango habitual en la literatura: 1,6–2,2 g/kg.
        /// </summary>
        public abstract double GramosProteinaPorKg { get; }

        /// <summary>
        /// Porcentaje de las calorías diarias que aporta la grasa. No baja del 20%:
        /// la grasa es necesaria para la síntesis hormonal y la absorción de vitaminas.
        /// </summary>
        public abstract double PorcentajeCaloriasDeGrasa { get; }
    }
}
