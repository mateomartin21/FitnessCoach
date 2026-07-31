namespace FitnessCoach.Domain.Models.Entrenamiento
{
    public enum TipoMedio
    {
        /// <summary>GIF animado. Se renderiza como &lt;img&gt;.</summary>
        Gif,

        /// <summary>Video de YouTube embebido. Se renderiza como &lt;iframe&gt;.</summary>
        VideoEmbebido,

        /// <summary>Enlace a una búsqueda de YouTube. No se embebe, se abre aparte.</summary>
        EnlaceBusqueda,

        /// <summary>Imagen local del proyecto. Último eslabón: no puede fallar.</summary>
        Placeholder
    }

    /// <summary>
    /// Una forma de mostrar un ejercicio. La vista intenta la primera y baja a la
    /// siguiente cuando el navegador reporta que no cargó.
    /// </summary>
    /// <param name="Tipo">Cómo debe renderizarse.</param>
    /// <param name="Url">De dónde sale el medio.</param>
    /// <param name="Descripcion">Texto alternativo, y leyenda cuando el medio no es visual.</param>
    public readonly record struct MedioEjercicio(TipoMedio Tipo, string Url, string Descripcion);
}
