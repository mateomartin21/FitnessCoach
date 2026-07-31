using FitnessCoach.Domain.Models.Gamificacion;
using Xunit;

namespace FitnessCoach.Tests.Gamificacion
{
    public class CalculadorMisionesTests
    {
        [Fact]
        public void SinActividadEstaSemana_NingunaMisionCumplida()
        {
            var misiones = CalculadorMisiones.Evaluar(EstadisticasUsuario.Vacias);

            Assert.All(misiones, m => Assert.False(m.Cumplida));
            Assert.Equal(0, CalculadorMisiones.XpCumplido(EstadisticasUsuario.Vacias));
        }

        [Fact]
        public void TresEntrenamientosEnLaSemana_CumplenLaConstanciaSemanal()
        {
            var e = EstadisticasUsuario.Vacias with { EntrenamientosEstaSemana = 3 };

            var mision = Buscar(CalculadorMisiones.Evaluar(e), "semana-entrenar");

            Assert.True(mision.Cumplida);
            Assert.Equal(100, mision.Porcentaje);
        }

        [Fact]
        public void ElProgresoParcialSeReporta()
        {
            // 2 de 4 días de diario: la misión va al 50%, aún sin cumplir.
            var e = EstadisticasUsuario.Vacias with { DiasConDiarioEstaSemana = 2 };

            var mision = Buscar(CalculadorMisiones.Evaluar(e), "semana-diario");

            Assert.False(mision.Cumplida);
            Assert.Equal(50, mision.Porcentaje);
        }

        [Fact]
        public void LasMisionesCumplidasSumanSuXp()
        {
            // pesarse (20) + constancia (50) = 70
            var e = EstadisticasUsuario.Vacias with { EntrenamientosEstaSemana = 3, RegistrosPesoEstaSemana = 1 };

            Assert.Equal(70, CalculadorMisiones.XpCumplido(e));
        }

        private static MisionEvaluada Buscar(IEnumerable<MisionEvaluada> misiones, string id) =>
            misiones.First(m => m.Mision.Id == id);
    }
}
