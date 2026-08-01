using FitnessCoach.Application.Coaching;
using Xunit;

namespace FitnessCoach.Tests.Coaching
{
    public class PersonalidadKodaTests
    {
        [Fact]
        public void ElPromptIncluyeElPerfilYLaPregunta()
        {
            var prompt = PersonalidadKoda.ConstruirPrompt(
                mensaje: "Cuanta proteina como?",
                contextoPerfil: "Nombre: Ana, Peso: 65kg, Objetivo: Perder grasa");

            Assert.Contains("Ana", prompt);
            Assert.Contains("65kg", prompt);
            Assert.Contains("Cuanta proteina como?", prompt);
        }

        [Fact]
        public void ElPromptFijaLaVozDelLobo()
        {
            var prompt = PersonalidadKoda.ConstruirPrompt("hola", "perfil");

            Assert.Contains("Koda", prompt);
            Assert.Contains("México", prompt);   // siempre responde en español de México
        }

        [Fact]
        public void ElPromptLeProhibeInventarAlimentosYEjercicios()
        {
            var prompt = PersonalidadKoda.ConstruirPrompt("recomendame algo", "perfil");

            Assert.Contains("NUNCA inventes", prompt);
            // El anclaje: solo lo que esté en el contexto (plan, rutina, catálogo).
            Assert.Contains("Solo puedes recomendar", prompt);
        }

        [Fact]
        public void ElPromptPideUsarLosDatosRealesDelUsuario()
        {
            var prompt = PersonalidadKoda.ConstruirPrompt("como voy?", "perfil");

            Assert.Contains("SUS datos", prompt);
        }

        [Theory]
        [InlineData("dieta", "plan")]
        [InlineData("rutina", "rutina")]
        [InlineData("progreso", "progreso")]
        [InlineData("semana", "ESTA SEMANA")]             // narra el resumen semanal
        [InlineData("cualquier-otra-cosa", "progreso")]   // cae en progreso por defecto
        public void ElPedidoDeAnalisis_ApuntaAlAspectoPedido(string aspecto, string esperado)
        {
            var pedido = PersonalidadKoda.PedidoDeAnalisis(aspecto);

            Assert.Contains(esperado, pedido, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void LaRespuestaSinSenal_EstaEnLaVozDelLobo_NoEsUnErrorTecnico()
        {
            var sinSenal = PersonalidadKoda.RespuestaSinSenal;

            Assert.Contains("campeón", sinSenal);
            // No debe filtrar jerga técnica al usuario.
            Assert.DoesNotContain("Exception", sinSenal);
            Assert.DoesNotContain("error", sinSenal, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("null", sinSenal, StringComparison.OrdinalIgnoreCase);
        }
    }
}
