using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Domain.Models
{
    public class RegistroProgreso
    {
        /// <summary>
        /// Identidad propia del registro. Antes existía solo como shadow property de EF,
        /// así que el dominio no podía referirse a un registro concreto para editarlo o borrarlo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>Siempre en UTC. La conversión a hora local se hace al mostrar.</summary>
        public DateTime Fecha { get; set; }

        [Range(RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        public double PesoKg { get; set; }

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        public string Notas { get; set; } = string.Empty;
    }
}
