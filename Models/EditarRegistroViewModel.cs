using System.ComponentModel.DataAnnotations;
using FitnessCoach.Domain.Models;

namespace FitnessCoach.Models
{
    /// <summary>
    /// Edición de un registro del historial. La fecha no se edita: es cuándo ocurrió el
    /// hecho, no un dato que el usuario ajuste. Se muestra solo como referencia.
    /// </summary>
    public class EditarRegistroViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Cuándo ocurrió, ya traducido a la zona del usuario (D-25). Solo para mostrar:
        /// no se persiste desde acá ni viaja en el POST, así que no hace falta el UTC.
        /// </summary>
        public DateTime FechaLocal { get; set; }

        [Range(RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        [Display(Name = "Peso (kg)")]
        public double PesoKg { get; set; }

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }
    }
}
