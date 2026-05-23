namespace FitnessCoach.Models.Entrenamiento
{
    public class Ejercicio
    {
        public string Nombre { get; set; } = string.Empty;
        public int Series { get; set; }
        public string Repeticiones { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }
}