namespace FitnessCoach.Domain.Models
{
    public class RegistroProgreso
    {
        public DateTime Fecha { get; set; }
        public double PesoKg { get; set; }
        public string Notas { get; set; } = string.Empty;
    }
}
