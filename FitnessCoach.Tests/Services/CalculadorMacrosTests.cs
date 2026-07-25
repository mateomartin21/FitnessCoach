using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Objetivos;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    public class CalculadorMacrosTests
    {
        private static UsuarioPerfil Usuario(double pesoKg, ObjetivoFitness? objetivo) => new()
        {
            Nombre = "Ana",
            Edad = 30,
            EstaturaCm = 165,
            PesoKg = pesoKg,
            ObjetivoActual = objetivo
        };

        [Fact]
        public void LaProteinaSaleDelPesoCorporal_NoDeLasCalorias()
        {
            // 70 kg × 2.2 g/kg (pérdida de grasa) = 154 g, sin importar las calorías.
            var macros = CalculadorMacros.Calcular(Usuario(70, new ObjetivoPerderPeso()), 2000);

            Assert.Equal(154, macros.ProteinaG);
        }

        [Fact]
        public void MismoPeso_DistintasCalorias_MismaProteina()
        {
            var conPocas = CalculadorMacros.Calcular(Usuario(70, new ObjetivoPerderPeso()), 1800);
            var conMuchas = CalculadorMacros.Calcular(Usuario(70, new ObjetivoPerderPeso()), 2600);

            Assert.Equal(conPocas.ProteinaG, conMuchas.ProteinaG);
            // Lo que cambia son los carbohidratos, que absorben la diferencia.
            Assert.True(conMuchas.CarbohidratoG > conPocas.CarbohidratoG);
        }

        [Fact]
        public void LaGrasaEsUnPorcentajeDeLasCalorias()
        {
            // 2000 kcal × 25% = 500 kcal / 9 = 55.6 → 56 g
            var macros = CalculadorMacros.Calcular(Usuario(70, new ObjetivoPerderPeso()), 2000);

            Assert.Equal(56, macros.GrasaG);
        }

        [Fact]
        public void LosMacrosSuman_AproximadamenteLasCaloriasObjetivo()
        {
            var macros = CalculadorMacros.Calcular(Usuario(80, new ObjetivoRecomposicion()), 2400);

            // El redondeo a gramos enteros mueve unas pocas kcal; más de 20 sería un error de reparto.
            Assert.InRange(macros.CaloriasSegunMacros, 2380, 2420);
        }

        [Theory]
        [InlineData(2.2, typeof(ObjetivoPerderPeso))]
        [InlineData(1.8, typeof(ObjetivoGanarMusculo))]
        [InlineData(2.0, typeof(ObjetivoRecomposicion))]
        public void CadaObjetivo_UsaSuFactorProteico(double factorEsperado, Type tipoObjetivo)
        {
            var objetivo = (ObjetivoFitness)Activator.CreateInstance(tipoObjetivo)!;

            var macros = CalculadorMacros.Calcular(Usuario(70, objetivo), 2200);

            Assert.Equal((int)Math.Round(70 * factorEsperado), macros.ProteinaG);
        }

        [Fact]
        public void PerderPeso_LlevaMasProteinaQueGanarMusculo()
        {
            // En déficit se protege la masa magra; en superávit no hace falta tanto.
            var deficit = CalculadorMacros.Calcular(Usuario(80, new ObjetivoPerderPeso()), 2200);
            var superavit = CalculadorMacros.Calcular(Usuario(80, new ObjetivoGanarMusculo()), 2200);

            Assert.True(deficit.ProteinaG > superavit.ProteinaG);
        }

        [Fact]
        public void SinObjetivo_UsaUnRepartoDeReferencia()
        {
            var macros = CalculadorMacros.Calcular(Usuario(70, objetivo: null), 2000);

            Assert.Equal(112, macros.ProteinaG);   // 70 × 1.6
            Assert.True(macros.CarbohidratoG > 0);
        }

        [Fact]
        public void LosCarbohidratosNuncaSonNegativos()
        {
            // Caso extremo: persona pesada con muy pocas calorías. Proteína y grasa
            // solas se comerían todo el presupuesto.
            var macros = CalculadorMacros.Calcular(Usuario(120, new ObjetivoPerderPeso()), 1200);

            Assert.True(macros.CarbohidratoG >= 0);
            Assert.True(macros.ProteinaG > 0);
        }

        [Fact]
        public void EnElCasoExtremo_SeRecortaLaProteinaPeroNoPorDebajoDelPiso()
        {
            var macros = CalculadorMacros.Calcular(Usuario(120, new ObjetivoPerderPeso()), 1200);

            // Piso de 1.2 g/kg: 120 × 1.2 = 144 g
            Assert.True(macros.ProteinaG >= 144);
        }

        [Fact]
        public void LosPorcentajesSonCoherentesConLosGramos()
        {
            var macros = CalculadorMacros.Calcular(Usuario(75, new ObjetivoRecomposicion()), 2500);

            var suma = macros.PorcentajeProteina + macros.PorcentajeGrasa + macros.PorcentajeCarbohidrato;
            Assert.InRange(suma, 98, 102);   // redondeo de tres porcentajes
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-500)]
        public void ConCaloriasInvalidas_Lanza(double calorias)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CalculadorMacros.Calcular(Usuario(70, new ObjetivoPerderPeso()), calorias));
        }

        [Fact]
        public void SinUsuario_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => CalculadorMacros.Calcular(null!, 2000));
        }
    }
}
