using System.ComponentModel.DataAnnotations;
using FitnessCoach.Domain.Models;
using Xunit;

namespace FitnessCoach.Tests.Domain
{
    /// <summary>
    /// Verifica las anotaciones del modelo — la primera capa de validación, la que
    /// evalúa el pipeline de MVC antes de que el controlador toque nada.
    /// La segunda capa (guardas del cálculo) se prueba en CalculadorCaloricoServiceTests.
    /// </summary>
    public class ValidacionUsuarioPerfilTests
    {
        private static IList<ValidationResult> Validar(object modelo)
        {
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(modelo, new ValidationContext(modelo), resultados, validateAllProperties: true);
            return resultados;
        }

        private static UsuarioPerfil PerfilValido() => new()
        {
            Nombre = "Ana",
            Edad = 30,
            PesoKg = 62,
            EstaturaCm = 165
        };

        [Fact]
        public void PerfilValido_NoTieneErrores()
        {
            Assert.Empty(Validar(PerfilValido()));
        }

        [Theory]
        [InlineData(-50)]
        [InlineData(0)]
        [InlineData(29.9)]
        [InlineData(300.1)]
        public void PesoFueraDeRango_EsInvalido(double pesoKg)
        {
            var perfil = PerfilValido();
            perfil.PesoKg = pesoKg;

            Assert.Contains(Validar(perfil), e => e.MemberNames.Contains(nameof(UsuarioPerfil.PesoKg)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(99)]
        [InlineData(251)]
        public void EstaturaFueraDeRango_EsInvalida(double estaturaCm)
        {
            var perfil = PerfilValido();
            perfil.EstaturaCm = estaturaCm;

            Assert.Contains(Validar(perfil), e => e.MemberNames.Contains(nameof(UsuarioPerfil.EstaturaCm)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(12)]
        [InlineData(101)]
        [InlineData(500)]
        public void EdadFueraDeRango_EsInvalida(int edad)
        {
            var perfil = PerfilValido();
            perfil.Edad = edad;

            Assert.Contains(Validar(perfil), e => e.MemberNames.Contains(nameof(UsuarioPerfil.Edad)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("A")]   // un solo carácter: por debajo del mínimo
        public void NombreInvalido_EsRechazado(string? nombre)
        {
            var perfil = PerfilValido();
            perfil.Nombre = nombre;

            Assert.Contains(Validar(perfil), e => e.MemberNames.Contains(nameof(UsuarioPerfil.Nombre)));
        }

        [Fact]
        public void RegistroProgreso_ConPesoFueraDeRango_EsInvalido()
        {
            var registro = new RegistroProgreso { Fecha = DateTime.UtcNow, PesoKg = -10 };

            Assert.Contains(Validar(registro), e => e.MemberNames.Contains(nameof(RegistroProgreso.PesoKg)));
        }

        [Fact]
        public void RegistroProgreso_ConNotasDemasiadoLargas_EsInvalido()
        {
            var registro = new RegistroProgreso
            {
                Fecha = DateTime.UtcNow,
                PesoKg = 70,
                Notas = new string('x', RangosPerfil.NotasLargoMaximo + 1)
            };

            Assert.Contains(Validar(registro), e => e.MemberNames.Contains(nameof(RegistroProgreso.Notas)));
        }

        [Fact]
        public void RegistroProgreso_EnElLimiteDeNotas_EsValido()
        {
            var registro = new RegistroProgreso
            {
                Fecha = DateTime.UtcNow,
                PesoKg = 70,
                Notas = new string('x', RangosPerfil.NotasLargoMaximo)
            };

            Assert.Empty(Validar(registro));
        }
    }
}
