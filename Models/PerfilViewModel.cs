using System.ComponentModel.DataAnnotations;
using FitnessCoach.Domain.Models;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Lo que el formulario de perfil puede enviar. Deliberadamente NO incluye
    /// Id ni IdentityUserId: el dueño se resuelve desde la sesión, no desde el POST.
    /// Los rangos salen de RangosPerfil, los mismos que valida el dominio.
    /// </summary>
    public class PerfilViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(RangosPerfil.NombreLargoMaximo, MinimumLength = RangosPerfil.NombreLargoMinimo,
            ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Range(RangosPerfil.EdadMinima, RangosPerfil.EdadMaxima,
            ErrorMessage = "La edad debe estar entre {1} y {2} años.")]
        [Display(Name = "Edad")]
        public int Edad { get; set; }

        [Range(RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        [Display(Name = "Peso (kg)")]
        public double PesoKg { get; set; }

        [Range(RangosPerfil.EstaturaMinimaCm, RangosPerfil.EstaturaMaximaCm,
            ErrorMessage = "La estatura debe estar entre {1} y {2} cm.")]
        [Display(Name = "Estatura (cm)")]
        public double EstaturaCm { get; set; }

        [Required]
        [Display(Name = "Objetivo Fitness")]
        public string TipoObjetivo { get; set; } = "Recomp";
    }
}
