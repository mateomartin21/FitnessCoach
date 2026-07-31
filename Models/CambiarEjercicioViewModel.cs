using FitnessCoach.Domain.Models.Entrenamiento;

namespace FitnessCoach.Models
{
    public class CambiarEjercicioViewModel
    {
        /// <summary>El slug que eligió la estrategia: con él se guarda el cambio.</summary>
        public string SlugReferencia { get; set; } = string.Empty;

        /// <summary>El que está hoy en la rutina, que puede ya ser un reemplazo.</summary>
        public Ejercicio EnUso { get; set; } = new();

        public bool EsSustituido { get; set; }

        /// <summary>Las que se muestran, ya recortadas a <see cref="Tope"/>.</summary>
        public IReadOnlyList<Ejercicio> Alternativas { get; set; } = Array.Empty<Ejercicio>();

        /// <summary>Cuántas hay en total: un grupo grande pasa de las cien.</summary>
        public int TotalAlternativas { get; set; }

        public string? Busqueda { get; set; }

        public const int Tope = 24;

        public bool HayMasDeLasQueSeVen => TotalAlternativas > Alternativas.Count;
    }
}
