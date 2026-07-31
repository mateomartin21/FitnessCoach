using System.ComponentModel.DataAnnotations;
using FitnessCoach.Domain.Models;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Lo que el formulario de progreso puede enviar. La fecha la pone el servidor.
    /// </summary>
    public class RegistrarPesoViewModel
    {
        [Range(RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        [Display(Name = "Nuevo peso (kg)")]
        public double NuevoPeso { get; set; }

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }
    }
}
