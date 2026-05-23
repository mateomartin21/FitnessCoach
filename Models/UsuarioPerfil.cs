using FitnessCoach.Models.Objetivos;

namespace FitnessCoach.Models
{
    public class UsuarioPerfil
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public double PesoKg { get; set; }
        public double EstaturaCm { get; set; }
        public int Edad { get; set; }
        public ObjetivoFitness? ObjetivoActual { get; set; }
        public List<RegistroProgreso> HistorialProgreso { get; set; } = new();
    }
}