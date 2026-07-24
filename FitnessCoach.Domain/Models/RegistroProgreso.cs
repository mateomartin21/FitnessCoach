using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Domain.Models
{
    public class RegistroProgreso
    {
        public DateTime Fecha { get; set; }

        [Range(30, 300, ErrorMessage = "El peso debe estar entre 30 y 300 kg.")]
        public double PesoKg { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden superar los 500 caracteres.")]
        public string Notas { get; set; } = string.Empty;
    }
}
