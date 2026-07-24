using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Domain.Models
{
    public class RegistroProgreso
    {
        public DateTime Fecha { get; set; }

        [Range(RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        public double PesoKg { get; set; }

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        public string Notas { get; set; } = string.Empty;
    }
}
