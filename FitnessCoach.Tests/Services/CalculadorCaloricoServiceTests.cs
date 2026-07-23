using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    public class CalculadorCaloricoServiceTests
    {
        private readonly CalculadorCaloricoService _calculador = new();

        [Fact]
        public void CalcularCaloriasDiarias_SinObjetivo_RetornaSoloMantenimiento()
        {
            // Arrange
            var usuario = new UsuarioPerfil
            {
                PesoKg = 70,
                EstaturaCm = 175,
                Edad = 30,
                ObjetivoActual = null
            };

            // Act
            double resultado = _calculador.CalcularCaloriasDiarias(usuario);

            // Assert
            // Mifflin-St Jeor: (10*70) + (6.25*175) - (5*30) + 5 = 1648.75
            // Mantenimiento: 1648.75 * 1.375 = 2267.03125
            Assert.Equal(2267.03125, resultado, precision: 3);
        }

        [Fact]
        public void CalcularCaloriasDiarias_ConObjetivoPerderPeso_AplicaMultiplicador085()
        {
            // Arrange
            var usuario = new UsuarioPerfil
            {
                PesoKg = 70,
                EstaturaCm = 175,
                Edad = 30,
                ObjetivoActual = new ObjetivoPerderPeso()
            };

            // Act
            double resultado = _calculador.CalcularCaloriasDiarias(usuario);

            // Assert
            // Mantenimiento (2267.03125) * 0.85 = 1926.9765625
            Assert.Equal(1926.9765625, resultado, precision: 3);
        }

        [Fact]
        public void CalcularCaloriasDiarias_ConObjetivoGanarMusculo_AplicaMultiplicador115()
        {
            // Arrange
            var usuario = new UsuarioPerfil
            {
                PesoKg = 80,
                EstaturaCm = 180,
                Edad = 25,
                ObjetivoActual = new ObjetivoGanarMusculo()
            };

            // Act
            double resultado = _calculador.CalcularCaloriasDiarias(usuario);

            // Assert
            double basal = (10 * 80) + (6.25 * 180) - (5 * 25) + 5;
            double mantenimiento = basal * 1.375;
            double esperado = mantenimiento * 1.15;

            Assert.Equal(esperado, resultado, precision: 3);
        }
    }
}