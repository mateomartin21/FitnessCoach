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

        /// <summary>
        /// El slug que había elegido la estrategia, cuando el usuario lo cambió por otro.
        /// Null = nadie lo tocó. Es la clave con la que la vista pide deshacer o volver a cambiar.
        /// </summary>
        public string? SlugOriginal { get; set; }

        public bool EsSustituido => SlugOriginal is not null;

        /// <summary>Con qué clave se guarda un cambio sobre esta fila.</summary>
        public string SlugDeReferencia => SlugOriginal ?? Ejercicio.Slug;

        /// <summary>Atajo de lectura, muy usado en las vistas y en las pruebas.</summary>
        public string Nombre => Ejercicio.Nombre;
    }
}
