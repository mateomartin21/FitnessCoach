namespace FitnessCoach.Models.Entrenamiento
{
    public class Rutina
    {
        public string NombreRutina { get; set; } = string.Empty;
        public string Nivel { get; set; } = "Intermedio";
        public List<DiaEntrenamiento> Dias { get; set; } = new();
    }
}