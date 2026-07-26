using FitnessCoach.Domain.Models.Gamificacion;
using Xunit;

namespace FitnessCoach.Tests.Gamificacion
{
    public class EvaluadorLogrosTests
    {
        [Fact]
        public void UsuarioNuevo_NoTieneNingunLogro()
        {
            var evaluados = EvaluadorLogros.Evaluar(EstadisticasUsuario.Vacias);

            Assert.All(evaluados, le => Assert.False(le.Desbloqueado));
            Assert.Equal(0, EvaluadorLogros.XpDesbloqueado(EstadisticasUsuario.Vacias));
        }

        [Fact]
        public void ConUnEntrenamiento_SeDesbloqueaElPrimerAullido()
        {
            var e = EstadisticasUsuario.Vacias with { TotalEntrenamientos = 1 };

            var primero = Buscar(EvaluadorLogros.Evaluar(e), "primer-entreno");

            Assert.True(primero.Desbloqueado);
        }

        [Fact]
        public void ElProgresoParcialSeReporta_AunqueNoEsteDesbloqueado()
        {
            // 4 de 10 entrenamientos: "En marcha" va al 40%, todavía bloqueado.
            var e = EstadisticasUsuario.Vacias with { TotalEntrenamientos = 4 };

            var enMarcha = Buscar(EvaluadorLogros.Evaluar(e), "diez-entrenos");

            Assert.False(enMarcha.Desbloqueado);
            Assert.Equal(4, enMarcha.ProgresoActual);
            Assert.Equal(40, enMarcha.Porcentaje);
        }

        [Fact]
        public void LosLogrosDesbloqueadosSumanSuXp()
        {
            // primer-entreno (25) + con-objetivo (20) = 45
            var e = EstadisticasUsuario.Vacias with { TotalEntrenamientos = 1, TieneObjetivo = true };

            Assert.Equal(45, EvaluadorLogros.XpDesbloqueado(e));
        }

        [Fact]
        public void ReciénDesbloqueados_SoloListaLosQueCruzaronElUmbral()
        {
            var antes = EstadisticasUsuario.Vacias with { TotalEntrenamientos = 9 };
            var despues = antes with { TotalEntrenamientos = 10 };

            var nuevos = EvaluadorLogros.ReciénDesbloqueados(antes, despues);

            Assert.Contains(nuevos, l => l.Id == "diez-entrenos");
            Assert.DoesNotContain(nuevos, l => l.Id == "primer-entreno");  // ya estaba
        }

        [Fact]
        public void CadaLogroTieneReaccionDelLobo()
        {
            Assert.All(CatalogoLogros.Todos, l => Assert.False(string.IsNullOrWhiteSpace(l.LineaLobo)));
        }

        private static LogroEvaluado Buscar(IEnumerable<LogroEvaluado> evaluados, string id) =>
            evaluados.First(le => le.Logro.Id == id);
    }
}
