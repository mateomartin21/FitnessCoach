using FitnessCoach.Application.Coaching;
using Xunit;

namespace FitnessCoach.Tests.Coaching
{
    public class PersonalidadLoboCoachTests
    {
        [Fact]
        public void ElPromptIncluyeElPerfilYLaPregunta()
        {
            var prompt = PersonalidadLoboCoach.ConstruirPrompt(
                mensaje: "Cuanta proteina como?",
                contextoPerfil: "Nombre: Ana, Peso: 65kg, Objetivo: Perder grasa");

            Assert.Contains("Ana", prompt);
            Assert.Contains("65kg", prompt);
            Assert.Contains("Cuanta proteina como?", prompt);
        }

        [Fact]
        public void ElPromptFijaLaVozDelLobo()
        {
            var prompt = PersonalidadLoboCoach.ConstruirPrompt("hola", "perfil");

            Assert.Contains("Lobo Coach", prompt);
            Assert.Contains("espanol", prompt);   // siempre responde en español
        }

        [Fact]
        public void ElPromptLeProhibeInventarAlimentosYEjercicios()
        {
            var prompt = PersonalidadLoboCoach.ConstruirPrompt("recomendame algo", "perfil");

            Assert.Contains("NUNCA inventes", prompt);
            // El anclaje: solo lo que esté en el contexto (plan, rutina, catálogo).
            Assert.Contains("Solo podes recomendar", prompt);
        }

        [Fact]
        public void ElPromptPideUsarLosDatosRealesDelUsuario()
        {
            var prompt = PersonalidadLoboCoach.ConstruirPrompt("como voy?", "perfil");

            Assert.Contains("SUS datos", prompt);
        }

        [Fact]
        public void LaRespuestaSinSenal_EstaEnLaVozDelLobo_NoEsUnErrorTecnico()
        {
            var sinSenal = PersonalidadLoboCoach.RespuestaSinSenal;

            Assert.Contains("campeon", sinSenal);
            // No debe filtrar jerga técnica al usuario.
            Assert.DoesNotContain("Exception", sinSenal);
            Assert.DoesNotContain("error", sinSenal, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("null", sinSenal, StringComparison.OrdinalIgnoreCase);
        }
    }
}
