using FitnessCoach.Application.Coaching;
using FitnessCoach.Domain.Ports;
using Xunit;

namespace FitnessCoach.Tests.Coaching
{
    public class CoachOfflineServiceTests
    {
        private readonly CoachOfflineService _offline = new();

        private async Task<string> Preguntar(string mensaje) =>
            await _offline.GenerarAsync(new ConsultaIA("prompt", mensaje, "perfil"));

        [Fact]
        public void EsUnProveedorDeRespaldo()
        {
            Assert.True(_offline.EsRespaldo);
            Assert.Equal("Offline", _offline.Nombre);
        }

        [Theory]
        [InlineData("cuanta proteina como al dia?", "proteina")]
        [InlineData("necesito descansar mas?", "descans")]
        [InlineData("no tengo ganas de entrenar hoy", "hoy")]
        [InlineData("me duele el hombro", "duele")]
        [InlineData("como armo mi rutina de pecho?", "tecnica")]
        public async Task RespondeSegunElTemaDeLaPregunta(string pregunta, string esperado)
        {
            var r = await Preguntar(pregunta);

            Assert.Contains(esperado, r, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SiempreAvisaQueEstaSinConexion()
        {
            // No hace pasar un consejo general por una respuesta a medida.
            var r = await Preguntar("cualquier cosa");

            Assert.Contains("senal", r, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RespondeEnLaVozDelLobo_SinJergaTecnica()
        {
            var r = await Preguntar("hola");

            Assert.Contains("campeon", r, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Exception", r);
            Assert.DoesNotContain("null", r, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("una pregunta sin ninguna palabra clave conocida")]
        public async Task NuncaFalla_NiSiquieraConMensajesVaciosOSinTema(string mensaje)
        {
            var r = await Preguntar(mensaje);

            Assert.False(string.IsNullOrWhiteSpace(r));
        }

        [Fact]
        public async Task UnMensajeNuloNoRompe()
        {
            var r = await _offline.GenerarAsync(new ConsultaIA("prompt", null!, "perfil"));

            Assert.False(string.IsNullOrWhiteSpace(r));
        }
    }
}
