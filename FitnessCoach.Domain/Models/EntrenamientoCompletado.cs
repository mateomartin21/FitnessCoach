using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Domain.Models
{
    /// <summary>
    /// Un entrenamiento que el usuario marcó como hecho. Es el hecho registrado,
    /// no la rutina planificada: por eso guarda el nombre como texto y no una
    /// referencia a <c>Rutina</c>, que hoy se genera al vuelo desde las estrategias.
    /// </summary>
    public class EntrenamientoCompletado
    {
        public int Id { get; set; }

        /// <summary>Siempre en UTC. La conversión a hora local se hace al mostrar.</summary>
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Indica qué entrenaste.")]
        [StringLength(RangosPerfil.NombreRutinaLargoMaximo,
            ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        public string NombreRutina { get; set; } = string.Empty;

        [Range(RangosPerfil.DuracionMinimaMin, RangosPerfil.DuracionMaximaMin,
            ErrorMessage = "La duración debe estar entre {1} y {2} minutos.")]
        public int DuracionMinutos { get; set; }

        [StringLength(RangosPerfil.NotasLargoMaximo,
            ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
        public string Notas { get; set; } = string.Empty;
    }
}
