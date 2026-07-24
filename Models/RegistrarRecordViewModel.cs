using System.ComponentModel.DataAnnotations;
using FitnessCoach.Domain.Models;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Marca lograda en un ejercicio. El ejercicio no se elige de una lista: viene
    /// del que el usuario estaba mirando, así que llega en campos ocultos.
    /// </summary>
    public class RegistrarRecordViewModel
    {
        [Required]
        public string EjercicioSlug { get; set; } = string.Empty;

        [Required]
        public string EjercicioNombre { get; set; } = string.Empty;

        [Range(RangosPerfil.PesoRecordMinimoKg, RangosPerfil.PesoRecordMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        [Display(Name = "Peso (kg)")]
        public double PesoKg { get; set; }

        [Range(1, RangosPerfil.RepeticionesMaximas,
            ErrorMessage = "Las repeticiones deben estar entre {1} y {2}.")]
        [Display(Name = "Repeticiones")]
        public int Repeticiones { get; set; } = 1;
    }
}
