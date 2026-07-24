using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Lo que el formulario de progreso puede enviar. La fecha la pone el servidor.
    /// </summary>
    public class RegistrarPesoViewModel
    {
        [Range(30, 300, ErrorMessage = "El peso debe estar entre 30 y 300 kg.")]
        [Display(Name = "Nuevo peso (kg)")]
        public double NuevoPeso { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden superar los 500 caracteres.")]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }
    }
}
