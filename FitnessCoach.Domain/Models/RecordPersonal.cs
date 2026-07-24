using System.ComponentModel.DataAnnotations;

namespace FitnessCoach.Domain.Models
{
    /// <summary>
    /// La mejor marca del usuario en un ejercicio concreto.
    /// Referencia al ejercicio por <c>Slug</c> y no por Id: el slug es estable entre
    /// entornos y sobrevive a que el catálogo se resiembre con otros identificadores.
    /// </summary>
    public class RecordPersonal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string EjercicioSlug { get; set; } = string.Empty;

        /// <summary>Copia del nombre al momento del récord, para poder mostrarlo sin consultar el catálogo.</summary>
        [StringLength(200)]
        public string EjercicioNombre { get; set; } = string.Empty;

        [Range(RangosPerfil.PesoRecordMinimoKg, RangosPerfil.PesoRecordMaximoKg,
            ErrorMessage = "El peso debe estar entre {1} y {2} kg.")]
        public double PesoKg { get; set; }

        [Range(1, RangosPerfil.RepeticionesMaximas,
            ErrorMessage = "Las repeticiones deben estar entre {1} y {2}.")]
        public int Repeticiones { get; set; }

        /// <summary>Cuándo se logró, en UTC.</summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Carga total del intento. Sirve para comparar series distintas del mismo
        /// ejercicio: 100 kg × 5 pesa más que 80 kg × 6.
        /// </summary>
        public double Volumen => PesoKg * Repeticiones;
    }
}
