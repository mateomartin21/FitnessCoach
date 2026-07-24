namespace FitnessCoach.Domain.Models.Entrenamiento
{
    public class DiaEntrenamiento
    {
        public string NombreDia { get; set; } = string.Empty;
        public string Enfoque { get; set; } = string.Empty;

        /// <summary>
        /// Ejercicios tal como los pide este día: el del catálogo más su prescripción.
        /// </summary>
        public List<EjercicioPrescrito> Ejercicios { get; set; } = new();
    }
}
