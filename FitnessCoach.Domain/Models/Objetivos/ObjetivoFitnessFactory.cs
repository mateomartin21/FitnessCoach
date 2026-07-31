namespace FitnessCoach.Domain.Models.Objetivos
{
    public static class ObjetivoFitnessFactory
    {
        public static ObjetivoFitness? CrearPorNombre(string? nombre)
        {
            return nombre switch
            {
                null => null,
                nameof(ObjetivoGanarMusculo) => new ObjetivoGanarMusculo(),
                nameof(ObjetivoPerderPeso) => new ObjetivoPerderPeso(),
                nameof(ObjetivoRecomposicion) => new ObjetivoRecomposicion(),
                _ => null
            };
        }

        public static string? ObtenerNombreTipo(ObjetivoFitness? objetivo) => objetivo?.GetType().Name;
    }
}