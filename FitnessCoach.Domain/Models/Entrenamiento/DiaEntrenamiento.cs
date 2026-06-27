namespace FitnessCoach.Domain.Models.Entrenamiento
{
    public class DiaEntrenamiento
    {
        public string NombreDia { get; set; } = string.Empty;
        public string Enfoque { get; set; } = string.Empty;
        public List<Ejercicio> Ejercicios { get; set; } = new();
    }
}
