using FitnessCoach.Domain.Models.Entrenamiento;
using Xunit;

namespace FitnessCoach.Tests.Domain
{
    public class FabricaMediosEjercicioTests
    {
        private static Ejercicio Ejercicio(string? gif = null, string? video = null) => new()
        {
            Slug = "dumbbell-biceps-curl",
            Nombre = "Curl de bíceps con mancuerna",
            UrlGif = gif,
            VideoYoutubeId = video
        };

        [Fact]
        public void ConGifYVideo_LaCadenaVaDelMejorAlMasResistente()
        {
            var medios = FabricaMediosEjercicio.Crear(
                Ejercicio(gif: "https://cdn.example/curl.gif", video: "abc123"));

            Assert.Equal(
                new[] { TipoMedio.Gif, TipoMedio.VideoEmbebido, TipoMedio.EnlaceBusqueda, TipoMedio.Placeholder },
                medios.Select(m => m.Tipo));
        }

        [Fact]
        public void SinGif_LaCadenaEmpiezaPorElVideo()
        {
            var medios = FabricaMediosEjercicio.Crear(Ejercicio(video: "abc123"));

            Assert.Equal(TipoMedio.VideoEmbebido, medios[0].Tipo);
            Assert.DoesNotContain(medios, m => m.Tipo == TipoMedio.Gif);
        }

        [Fact]
        public void SinNingunMedio_TodaviaHayEnlaceYPlaceholder()
        {
            // El caso que importa: un ejercicio sin material igual se muestra completo.
            var medios = FabricaMediosEjercicio.Crear(Ejercicio());

            Assert.Equal(
                new[] { TipoMedio.EnlaceBusqueda, TipoMedio.Placeholder },
                medios.Select(m => m.Tipo));
        }

        [Fact]
        public void LaCadenaSiempreTerminaEnUnPlaceholderLocal()
        {
            foreach (var ejercicio in new[] { Ejercicio(), Ejercicio(gif: "x"), Ejercicio(video: "y") })
            {
                var ultimo = FabricaMediosEjercicio.Crear(ejercicio)[^1];

                Assert.Equal(TipoMedio.Placeholder, ultimo.Tipo);
                Assert.StartsWith("/", ultimo.Url);   // ruta local, no depende de ninguna red
            }
        }

        [Fact]
        public void ElEnlaceDeBusqueda_EscapaElTerminoParaNoRomperLaUrl()
        {
            var medios = FabricaMediosEjercicio.Crear(Ejercicio());
            var enlace = medios.First(m => m.Tipo == TipoMedio.EnlaceBusqueda);

            Assert.DoesNotContain(" ", enlace.Url);
            Assert.Contains("youtube.com/results", enlace.Url);
        }

        [Fact]
        public void ElVideoUsaElDominioSinCookies()
        {
            var medios = FabricaMediosEjercicio.Crear(Ejercicio(video: "abc123"));
            var video = medios.First(m => m.Tipo == TipoMedio.VideoEmbebido);

            Assert.Contains("youtube-nocookie.com/embed/abc123", video.Url);
        }

        [Fact]
        public void SinEjercicio_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => FabricaMediosEjercicio.Crear(null!));
        }
    }
}
