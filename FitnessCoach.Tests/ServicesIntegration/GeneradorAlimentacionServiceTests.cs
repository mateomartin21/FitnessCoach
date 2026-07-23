using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models.Objetivos;
using Xunit;

namespace FitnessCoach.Tests.ServicesIntegration
{
    public class GeneradorAlimentacionServiceTests
    {
        private readonly GeneradorAlimentacionService _generador = new();

        [Fact]
        public void GenerarPlanParaObjetivo_ConPerderPeso_SeleccionaEstrategiaCorrecta()
        {
            // Act
            var plan = _generador.GenerarPlanParaObjetivo(new ObjetivoPerderPeso());

            // Assert
            Assert.Equal("Pérdida de Grasa", plan.Objetivo);
        }

        [Fact]
        public void GenerarPlanParaObjetivo_ConGanarMusculo_SeleccionaEstrategiaCorrecta()
        {
            // Act
            var plan = _generador.GenerarPlanParaObjetivo(new ObjetivoGanarMusculo());

            // Assert
            Assert.Equal("Ganancia Muscular", plan.Objetivo);
        }

        [Fact]
        public void GenerarPlanParaObjetivo_SiempreIncluyeRecomendacionesDeHidratacion()
        {
            // Act — sin importar el objetivo, el decorator de hidratación siempre debe aplicarse
            var plan = _generador.GenerarPlanParaObjetivo(new ObjetivoRecomposicion());

            // Assert
            Assert.Contains("Tomar 500ml de agua al despertar en ayunas", plan.RecomendacionesGenerales);
        }
    }
}