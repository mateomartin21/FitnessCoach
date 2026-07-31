using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Models
{
    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "Escribe tu contraseña actual.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ContrasenaNueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "Repite la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Repite la nueva contraseña")]
        [Compare(nameof(ContrasenaNueva), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}
