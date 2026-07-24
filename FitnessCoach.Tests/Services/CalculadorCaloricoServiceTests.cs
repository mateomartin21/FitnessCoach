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

        // --- Guardas: el cálculo se niega a operar sobre datos inválidos ---

        [Fact]
        public void CalcularCaloriasDiarias_ConEstaturaCero_Lanza()
        {
            // Arrange — el bug silencioso original: la fórmula devolvía un número
            // perfectamente formado y perfectamente falso en vez de fallar.
            var usuario = new UsuarioPerfil { PesoKg = 70, EstaturaCm = 0, Edad = 30 };

            // Act + Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => _calculador.CalcularCaloriasDiarias(usuario));
            Assert.Contains("EstaturaCm", ex.Message);
        }

        [Theory]
        [InlineData(-50)]   // peso negativo
        [InlineData(0)]
        [InlineData(29.9)]  // apenas por debajo del mínimo
        [InlineData(300.1)] // apenas por encima del máximo
        public void CalcularCaloriasDiarias_ConPesoFueraDeRango_Lanza(double pesoKg)
        {
            var usuario = new UsuarioPerfil { PesoKg = pesoKg, EstaturaCm = 175, Edad = 30 };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _calculador.CalcularCaloriasDiarias(usuario));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(12)]    // menor que la edad mínima
        [InlineData(101)]
        [InlineData(500)]
        public void CalcularCaloriasDiarias_ConEdadFueraDeRango_Lanza(int edad)
        {
            var usuario = new UsuarioPerfil { PesoKg = 70, EstaturaCm = 175, Edad = edad };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _calculador.CalcularCaloriasDiarias(usuario));
        }

        [Theory]
        [InlineData(99.9)]
        [InlineData(250.1)]
        public void CalcularCaloriasDiarias_ConEstaturaFueraDeRango_Lanza(double estaturaCm)
        {
            var usuario = new UsuarioPerfil { PesoKg = 70, EstaturaCm = estaturaCm, Edad = 30 };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _calculador.CalcularCaloriasDiarias(usuario));
        }

        [Theory]
        [InlineData(RangosPerfil.PesoMinimoKg, RangosPerfil.EstaturaMinimaCm, RangosPerfil.EdadMinima)]
        [InlineData(RangosPerfil.PesoMaximoKg, RangosPerfil.EstaturaMaximaCm, RangosPerfil.EdadMaxima)]
        public void CalcularCaloriasDiarias_EnLosLimitesExactos_Calcula(double peso, double estatura, int edad)
        {
            // Arrange — los extremos son válidos: la guarda excluye lo que está FUERA del rango
            var usuario = new UsuarioPerfil { PesoKg = peso, EstaturaCm = estatura, Edad = edad };

            // Act
            double resultado = _calculador.CalcularCaloriasDiarias(usuario);

            // Assert
            Assert.True(resultado > 0);
        }

        [Fact]
        public void CalcularCaloriasDiarias_SinUsuario_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => _calculador.CalcularCaloriasDiarias(null!));
        }
    }
}