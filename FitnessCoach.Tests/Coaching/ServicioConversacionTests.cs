using FitnessCoach.Application.Coaching;
using FitnessCoach.Domain.Models.Coaching;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Coaching
{
    public class ServicioConversacionTests
    {
        private const int Mateo = 1;
        private const int Otro = 2;

        private static (ServicioConversacion servicio, RepositorioConversacionFalso repo) Crear()
        {
            var repo = new RepositorioConversacionFalso();
            return (new ServicioConversacion(repo), repo);
        }

        [Fact]
        public void RegistrarIntercambio_GuardaLaPreguntaYLaRespuesta_EnEseOrden()
        {
            var (servicio, _) = Crear();

            servicio.RegistrarIntercambio(Mateo, "¿cómo voy?", "Vas bien, campeón.");

            var historial = servicio.Historial(Mateo);
            Assert.Equal(2, historial.Count);
            Assert.False(historial[0].EsDeKoda);
            Assert.Equal("¿cómo voy?", historial[0].Texto);
            Assert.True(historial[1].EsDeKoda);
            Assert.Equal("Vas bien, campeón.", historial[1].Texto);
        }

        [Fact]
        public void ElHistorialVaDelMasViejoAlMasNuevo()
        {
            var (servicio, _) = Crear();

            servicio.RegistrarIntercambio(Mateo, "primera", "respuesta uno");
            servicio.RegistrarIntercambio(Mateo, "segunda", "respuesta dos");

            var textos = servicio.Historial(Mateo).Select(m => m.Texto).ToArray();
            Assert.Equal(new[] { "primera", "respuesta uno", "segunda", "respuesta dos" }, textos);
        }

        [Fact]
        public void LaConversacionDeUnUsuarioNoSeMezclaConLaDeOtro()
        {
            var (servicio, _) = Crear();

            servicio.RegistrarIntercambio(Mateo, "lo mío", "para Mateo");
            servicio.RegistrarIntercambio(Otro, "lo suyo", "para el otro");

            Assert.All(servicio.Historial(Mateo), m => Assert.DoesNotContain("otro", m.Texto));
            Assert.Equal(2, servicio.Historial(Otro).Count);
        }

        // Sin tope, la tabla crece para siempre en una conversacion que nadie cierra.
        [Fact]
        public void LaConversacionNoCreceMasAllaDelTope()
        {
            var (servicio, _) = Crear();

            for (var i = 0; i < MensajeCoach.MaximoGuardados; i++)
                servicio.RegistrarIntercambio(Mateo, $"pregunta {i}", $"respuesta {i}");

            var historial = servicio.Historial(Mateo);
            Assert.Equal(MensajeCoach.MaximoGuardados, historial.Count);

            // Y lo que queda es lo ULTIMO, no lo primero: se poda por el otro extremo.
            Assert.Equal($"respuesta {MensajeCoach.MaximoGuardados - 1}", historial[^1].Texto);
        }

        // Lo que ve el usuario en pantalla y lo que se le manda al modelo son dos cosas:
        // el historial completo se muestra, pero solo unos turnos viajan en el prompt.
        [Fact]
        public void LaMemoriaEsMasCortaQueElHistorial()
        {
            var (servicio, _) = Crear();

            for (var i = 0; i < 10; i++)
                servicio.RegistrarIntercambio(Mateo, $"pregunta {i}", $"respuesta {i}");

            Assert.Equal(MensajeCoach.MensajesDeMemoria, servicio.Memoria(Mateo).Count);
            Assert.Equal(20, servicio.Historial(Mateo).Count);

            // Y la memoria es la parte final de la charla, no el principio.
            Assert.Equal("respuesta 9", servicio.Memoria(Mateo)[^1].Texto);
        }

        [Theory]
        [InlineData("", "algo")]
        [InlineData("   ", "algo")]
        [InlineData("algo", "")]
        [InlineData("algo", "   ")]
        public void NoSeGuardaUnIntercambioAMedias(string pregunta, string respuesta)
        {
            var (servicio, repo) = Crear();

            servicio.RegistrarIntercambio(Mateo, pregunta, respuesta);

            Assert.Empty(servicio.Historial(Mateo));
            Assert.Empty(repo.Todo);
        }

        [Fact]
        public void Borrar_DejaLaCharlaVacia_YNoTocaLaDeOtroUsuario()
        {
            var (servicio, _) = Crear();
            servicio.RegistrarIntercambio(Mateo, "hola", "qué tal");
            servicio.RegistrarIntercambio(Otro, "hola", "qué tal");

            servicio.Borrar(Mateo);

            Assert.Empty(servicio.Historial(Mateo));
            Assert.NotEmpty(servicio.Historial(Otro));
        }

        // La respuesta de un modelo no tiene largo garantizado; si supera la columna,
        // la insercion falla y se pierde el intercambio entero.
        [Fact]
        public void UnaRespuestaEnormeSeRecorta_EnVezDeRomperLaInsercion()
        {
            var (servicio, _) = Crear();
            var enorme = new string('a', MensajeCoach.TextoLargoMaximo + 500);

            servicio.RegistrarIntercambio(Mateo, "dime todo", enorme);

            var respuesta = servicio.Historial(Mateo)[^1];
            Assert.Equal(MensajeCoach.TextoLargoMaximo, respuesta.Texto.Length);
        }
    }
}
