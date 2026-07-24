namespace FitnessCoach.Domain.Models.Entrenamiento
{
    /// <summary>
    /// Lo que un día de rutina pide de un ejercicio del catálogo: cuántas series,
    /// cuántas repeticiones y con qué indicaciones. El mismo <see cref="Ejercicio"/>
    /// se prescribe distinto según el objetivo.
    /// </summary>
    public class EjercicioPrescrito
    {
        public Ejercicio Ejercicio { get; set; } = new();

        public int Series { get; set; }

        /// <summary>Texto libre porque admite rangos y consignas: "8-10", "Al fallo", "30 seg".</summary>
        public string Repeticiones { get; set; } = string.Empty;

        public string Notas { get; set; } = string.Empty;

        /// <summary>Atajo de lectura, muy usado en las vistas y en las pruebas.</summary>
        public string Nombre => Ejercicio.Nombre;
    }
}
