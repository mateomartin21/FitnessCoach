using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models.Objetivos;
using Xunit;

namespace FitnessCoach.Tests.ServicesIntegration
{
    public class GeneradorRutinasServiceTests
    {
        private readonly GeneradorRutinasService _generador = new();

        [Fact]
        public void GenerarRutinaParaObjetivo_ConPerderPeso_SeleccionaEstrategiaCorrecta()
        {
            // Act
            var rutina = _generador.GenerarRutinaParaObjetivo(new ObjetivoPerderPeso());

            // Assert
            Assert.Equal("Principiante/Intermedio", rutina.Nivel);
        }

        [Fact]
        public void GenerarRutinaParaObjetivo_ConGanarMusculo_SeleccionaEstrategiaCorrecta()
        {
            // Act
            var rutina = _generador.GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo());

            // Assert
            Assert.Equal("Avanzado", rutina.Nivel);
        }

        [Fact]
        public void GenerarRutinaParaObjetivo_ConRecomposicion_SeleccionaEstrategiaCorrecta()
        {
            // Act
            var rutina = _generador.GenerarRutinaParaObjetivo(new ObjetivoRecomposicion());

            // Assert
            Assert.Equal("Intermedio", rutina.Nivel);
        }

        [Fact]
        public void GenerarRutinaParaObjetivo_TodosLosDiasTienenCalentamientoYEnfriamiento()
        {
            // Act
            var rutina = _generador.GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo());

            // Assert — el Decorator debe aplicarse a CADA día, no solo al primero
            Assert.All(rutina.Dias, dia =>
            {
                Assert.Equal("Calentamiento General", dia.Ejercicios[0].Nombre);
                Assert.Equal("Enfriamiento y Estiramientos", dia.Ejercicios[^1].Nombre);
            });
        }
    }
}