namespace FitnessCoach.Domain.Models.Entrenamiento
{
    /// <summary>
    /// Un ejercicio del catálogo: qué es, no cuánto hacer de él.
    /// Las series y repeticiones dependen de la rutina que lo use, así que viven
    /// en <see cref="EjercicioPrescrito"/> y no acá.
    /// </summary>
    public class Ejercicio
    {
        public int Id { get; set; }

        /// <summary>Identificador estable y legible (ej. "dumbbell-biceps-curl"). Único.</summary>
        public string Slug { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        /// <summary>Músculo principal (ej. "biceps").</summary>
        public string GrupoMuscular { get; set; } = string.Empty;

        /// <summary>Zona del cuerpo (ej. "arms").</summary>
        public string ParteCuerpo { get; set; } = string.Empty;

        /// <summary>Equipo necesario (ej. "dumbbell", "body weight").</summary>
        public string Equipo { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public List<string> MusculosSecundarios { get; set; } = new();

        public List<string> Instrucciones { get; set; } = new();

        /// <summary>GIF demostrativo. Puede faltar: la vista degrada con la cadena de medios.</summary>
        public string? UrlGif { get; set; }

        /// <summary>Id del video de YouTube para embeber. Puede faltar.</summary>
        public string? VideoYoutubeId { get; set; }

        /// <summary>
        /// Término de búsqueda para el enlace de respaldo a YouTube. Se arma con el
        /// nombre porque una búsqueda nunca queda rota, a diferencia de un id inventado.
        /// </summary>
        public string TerminoBusquedaVideo => $"{Nombre} técnica correcta";
    }
}
