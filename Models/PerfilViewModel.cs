using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Lo que el formulario de perfil puede enviar. Deliberadamente NO incluye
    /// Id ni IdentityUserId: el dueño se resuelve desde la sesión, no desde el POST.
    /// </summary>
    public class PerfilViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Range(13, 100, ErrorMessage = "La edad debe estar entre 13 y 100 años.")]
        [Display(Name = "Edad")]
        public int Edad { get; set; }

        [Range(30, 300, ErrorMessage = "El peso debe estar entre 30 y 300 kg.")]
        [Display(Name = "Peso (kg)")]
        public double PesoKg { get; set; }

        [Range(100, 250, ErrorMessage = "La estatura debe estar entre 100 y 250 cm.")]
        [Display(Name = "Estatura (cm)")]
        public double EstaturaCm { get; set; }

        [Required]
        [Display(Name = "Objetivo Fitness")]
        public string TipoObjetivo { get; set; } = "Recomp";
    }
}
