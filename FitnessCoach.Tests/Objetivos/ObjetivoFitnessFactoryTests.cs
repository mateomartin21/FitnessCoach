using FitnessCoach.Domain.Models.Objetivos;
using Xunit;

namespace FitnessCoach.Tests.Objetivos
{
    public class ObjetivoFitnessFactoryTests
    {
        [Fact]
        public void CrearPorNombre_ConNombreGanarMusculo_RetornaObjetivoGanarMusculo()
        {
            // Act
            var objetivo = ObjetivoFitnessFactory.CrearPorNombre(nameof(ObjetivoGanarMusculo));

            // Assert
            Assert.IsType<ObjetivoGanarMusculo>(objetivo);
        }

        [Fact]
        public void CrearPorNombre_ConNombrePerderPeso_RetornaObjetivoPerderPeso()
        {
            // Act
            var objetivo = ObjetivoFitnessFactory.CrearPorNombre(nameof(ObjetivoPerderPeso));

            // Assert
            Assert.IsType<ObjetivoPerderPeso>(objetivo);
        }

        [Fact]
        public void CrearPorNombre_ConNombreRecomposicion_RetornaObjetivoRecomposicion()
        {
            // Act
            var objetivo = ObjetivoFitnessFactory.CrearPorNombre(nameof(ObjetivoRecomposicion));

            // Assert
            Assert.IsType<ObjetivoRecomposicion>(objetivo);
        }

        [Fact]
        public void CrearPorNombre_ConNombreNulo_RetornaNull()
        {
            // Act
            var objetivo = ObjetivoFitnessFactory.CrearPorNombre(null);

            // Assert
            Assert.Null(objetivo);
        }

        [Fact]
        public void CrearPorNombre_ConNombreDesconocido_RetornaNull()
        {
            // Act
            var objetivo = ObjetivoFitnessFactory.CrearPorNombre("ObjetivoQueNoExiste");

            // Assert
            Assert.Null(objetivo);
        }

        [Fact]
        public void ObtenerNombreTipo_ConObjetivoValido_RetornaNombreDeLaClase()
        {
            // Arrange
            var objetivo = new ObjetivoGanarMusculo();

            // Act
            var nombreTipo = ObjetivoFitnessFactory.ObtenerNombreTipo(objetivo);

            // Assert
            Assert.Equal(nameof(ObjetivoGanarMusculo), nombreTipo);
        }
    }
}