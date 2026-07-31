using System.ComponentModel.DataAnnotations;
using FitnessCoach.Domain.Models;

namespace FitnessCoach.Models
{
    /// <summary>Marcar un entrenamiento como hecho. La fecha la pone el servidor.</summary>
    public class RegistrarEntrenamientoViewModel
    {
        [Required(ErrorMessage = "Indicá qué entrenaste.")]
        [StringLength(RangosPerfil.NombreRutinaLargoMaximo,
            ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "¿Qué entrenaste?")]
        public string NombreRutina { get; set; } = string.Empty;

        [Range(RangosPerfil.DuracionMinimaMin, RangosPerfil.DuracionMaximaMin,
            ErrorMessage = "La duración debe estar entre {1} y {2} minutos.")]
        [Display(Name = "Duración (minutos)")]
        public int DuracionMinutos { get; set; } = 45;

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }
    }
}
