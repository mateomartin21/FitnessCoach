using FitnessCoach.Domain.Models.Alimentacion;
using Xunit;

namespace FitnessCoach.Tests.Domain
{
    public class ResumenDiarioTests
    {
        private static readonly DateOnly Hoy = new(2026, 7, 25);
        private static readonly ObjetivoMacros Objetivo = new(2000, 150, 56, 200);

        private static RegistroComida Registro(double kcal, double prot, double carbo, double grasa) => new()
        {
            Fecha = Hoy.ToDateTime(TimeOnly.MinValue),
            AlimentoSlug = "x", AlimentoNombre = "X", Gramos = 100,
            Calorias = kcal, ProteinaG = prot, CarbohidratoG = carbo, GrasaG = grasa
        };

        [Fact]
        public void SumaLosMacrosDeLosRegistros()
        {
            var resumen = new ResumenDiario(Hoy, Objetivo, new[]
            {
                Registro(500, 40, 50, 15),
                Registro(300, 20, 30, 10)
            });

            Assert.Equal(800, resumen.CaloriasConsumidas);
            Assert.Equal(60, resumen.ProteinaConsumidaG);
        }

        [Fact]
        public void LoRestanteEsElObjetivoMenosLoConsumido()
        {
            var resumen = new ResumenDiario(Hoy, Objetivo, new[] { Registro(800, 50, 80, 25) });

            Assert.Equal(1200, resumen.CaloriasRestantes);
            Assert.Equal(100, resumen.ProteinaRestanteG);
        }

        [Fact]
        public void LoRestanteNuncaEsNegativo_AunqueUnoSePase()
        {
            var resumen = new ResumenDiario(Hoy, Objetivo, new[] { Registro(2500, 200, 250, 80) });

            Assert.Equal(0, resumen.CaloriasRestantes);
            Assert.Equal(0, resumen.ProteinaRestanteG);
        }

        [Fact]
        public void DetectaCuandoSePasoDelObjetivo()
        {
            // Con un 5% de margen: 2000 → se pasa recién por encima de 2100.
            var justo = new ResumenDiario(Hoy, Objetivo, new[] { Registro(2050, 150, 200, 56) });
            var pasado = new ResumenDiario(Hoy, Objetivo, new[] { Registro(2300, 150, 260, 56) });

            Assert.False(justo.SePaso);
            Assert.True(pasado.SePaso);
        }

        [Fact]
        public void ElPorcentajeCaloricoReflejaLoConsumido()
        {
            var resumen = new ResumenDiario(Hoy, Objetivo, new[] { Registro(1000, 75, 100, 28) });

            Assert.Equal(50, resumen.PorcentajeCalorico);
        }

        [Fact]
        public void UnDiaSinRegistros_EstaVacioYNoDividePorCero()
        {
            var resumen = new ResumenDiario(Hoy, Objetivo, Array.Empty<RegistroComida>());

            Assert.True(resumen.SinRegistros);
            Assert.Equal(0, resumen.CaloriasConsumidas);
            Assert.Equal(2000, resumen.CaloriasRestantes);
        }

        [Fact]
        public void ConObjetivoEnCero_NoDividePorCero()
        {
            // Pasa si el perfil aún no tiene datos válidos para calcular calorías.
            var resumen = new ResumenDiario(Hoy, default, new[] { Registro(500, 40, 50, 15) });

            Assert.Equal(0, resumen.PorcentajeCalorico);
            Assert.Equal(500, resumen.CaloriasConsumidas);
        }
    }
}
