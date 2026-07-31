namespace FitnessCoach.Domain.Models.Entrenamiento
{
    /// <summary>
    /// Arma la cadena de medios de un ejercicio, del mejor al más resistente:
    /// GIF → video embebido → enlace de búsqueda → placeholder local.
    ///
    /// El servidor no puede saber si un GIF remoto va a cargar en el navegador del
    /// usuario, así que en vez de elegir uno entrega la cadena entera y la vista va
    /// bajando con onerror. El último eslabón es un asset propio: **siempre** existe,
    /// y por eso nunca se llega a ver el ícono de imagen rota.
    ///
    /// Agregar una fuente nueva (otro CDN, otro proveedor de video) es sumar un
    /// eslabón acá; ni la vista ni las estrategias se enteran.
    /// </summary>
    public static class FabricaMediosEjercicio
    {
        /// <summary>Último eslabón: un asset propio que siempre existe (D-31).</summary>
        public const string RutaPlaceholder = "/images/koda/koda-pensativo.png";

        private const string BaseBusquedaYoutube = "https://www.youtube.com/results?search_query=";
        private const string BaseEmbedYoutube = "https://www.youtube-nocookie.com/embed/";

        public static IReadOnlyList<MedioEjercicio> Crear(Ejercicio ejercicio)
        {
            ArgumentNullException.ThrowIfNull(ejercicio);

            var cadena = new List<MedioEjercicio>();

            if (!string.IsNullOrWhiteSpace(ejercicio.UrlGif))
                cadena.Add(new MedioEjercicio(TipoMedio.Gif, ejercicio.UrlGif, ejercicio.Nombre));

            if (!string.IsNullOrWhiteSpace(ejercicio.VideoYoutubeId))
                cadena.Add(new MedioEjercicio(
                    TipoMedio.VideoEmbebido,
                    BaseEmbedYoutube + ejercicio.VideoYoutubeId,
                    $"Video: {ejercicio.Nombre}"));

            // Una búsqueda no puede quedar rota, a diferencia de un id de video inventado.
            cadena.Add(new MedioEjercicio(
                TipoMedio.EnlaceBusqueda,
                BaseBusquedaYoutube + Uri.EscapeDataString(ejercicio.TerminoBusquedaVideo),
                $"Ver técnica de {ejercicio.Nombre} en YouTube"));

            cadena.Add(new MedioEjercicio(TipoMedio.Placeholder, RutaPlaceholder, ejercicio.Nombre));

            return cadena;
        }
    }
}
