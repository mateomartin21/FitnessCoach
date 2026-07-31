using FitnessCoach.Application.Coaching;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Coaching
{
    public class CoachResilienteTests
    {
        private readonly LoggerCapturador<CoachResiliente> _log = new();

        private CoachResiliente Coach(params IProveedorIA[] proveedores) => new(proveedores, _log);

        [Fact]
        public async Task UsaLaRespuestaDelPrimerProveedorQueContesta()
        {
            var coach = Coach(
                ProveedorIAFalso.QueResponde("Dale que se puede, campeon.", "Gemini"),
                ProveedorIAFalso.QueResponde("respaldo", "Offline", esRespaldo: true));

            var r = await coach.ConsultarAsync("hola", "perfil");

            Assert.Equal("Dale que se puede, campeon.", r.Texto);
            Assert.Equal("Gemini", r.Fuente);
            Assert.False(r.EsDegradada);
        }

        [Fact]
        public async Task SiElPrimeroFalla_PasaAlSiguiente()
        {
            var segundo = ProveedorIAFalso.QueResponde("Yo te cubro.", "Offline", esRespaldo: true);
            var coach = Coach(ProveedorIAFalso.QueFalla("Gemini"), segundo);

            var r = await coach.ConsultarAsync("hola", "perfil");

            Assert.Equal("Yo te cubro.", r.Texto);
            Assert.Equal("Offline", r.Fuente);
            Assert.True(r.EsDegradada);          // vino del respaldo
            Assert.Equal(1, segundo.VecesLlamado);
        }

        [Fact]
        public async Task ElFalloDelPrimeroQuedaRegistrado()
        {
            var coach = Coach(
                ProveedorIAFalso.QueFalla("Gemini"),
                ProveedorIAFalso.QueResponde("ok", "Offline"));

            await coach.ConsultarAsync("hola", "perfil");

            // Antes (D-09) los fallos no se registraban en ningún lado.
            Assert.True(_log.Advertencias >= 1);
            Assert.Contains(_log.Registros, r => r.Mensaje.Contains("Gemini"));
        }

        [Fact]
        public async Task SiTodosFallan_ElLoboRespondeIgual_YNoEsUnErrorCrudo()
        {
            var coach = Coach(
                ProveedorIAFalso.QueFalla("Gemini"),
                ProveedorIAFalso.QueFalla("Offline"));

            var r = await coach.ConsultarAsync("hola", "perfil");

            Assert.Equal(PersonalidadLoboCoach.RespuestaSinSenal, r.Texto);
            Assert.True(r.EsDegradada);
            Assert.DoesNotContain("Exception", r.Texto);
            Assert.Equal(1, _log.Errores);       // el fallo total se registró como error
        }

        [Fact]
        public async Task UnaRespuestaVacia_SeTrataComoFallo_YSigueLaCadena()
        {
            var coach = Coach(
                ProveedorIAFalso.QueDevuelveVacio("Gemini"),
                ProveedorIAFalso.QueResponde("Aca estoy.", "Offline"));

            var r = await coach.ConsultarAsync("hola", "perfil");

            Assert.Equal("Aca estoy.", r.Texto);
            Assert.Equal("Offline", r.Fuente);
        }

        [Fact]
        public async Task NoLlamaAlSegundoProveedor_SiElPrimeroYaRespondio()
        {
            var segundo = ProveedorIAFalso.QueResponde("no deberia usarse", "Offline");
            var coach = Coach(ProveedorIAFalso.QueResponde("listo", "Gemini"), segundo);

            await coach.ConsultarAsync("hola", "perfil");

            Assert.Equal(0, segundo.VecesLlamado);
        }

        [Fact]
        public async Task SinNingunProveedor_DevuelveLaRespuestaDegradada()
        {
            var coach = Coach();   // lista vacía

            var r = await coach.ConsultarAsync("hola", "perfil");

            Assert.True(r.EsDegradada);
            Assert.Equal(PersonalidadLoboCoach.RespuestaSinSenal, r.Texto);
        }

        [Fact]
        public void SinLogger_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => new CoachResiliente(Array.Empty<IProveedorIA>(), null!));
        }
    }
}
