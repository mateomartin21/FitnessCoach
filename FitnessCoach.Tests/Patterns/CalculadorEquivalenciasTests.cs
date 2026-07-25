using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Patterns.Strategy.Alimentacion;
using Xunit;

namespace FitnessCoach.Tests.Patterns
{
    public class CalculadorEquivalenciasTests
    {
        private static Alimento Alimento(
            string slug, string categoria, string grupo,
            double prot, double carbo, double grasa,
            double min = 30, double max = 300) => new()
        {
            Slug = slug, Nombre = slug, Categoria = categoria, GrupoIntercambio = grupo,
            ProteinaPor100g = prot, CarbohidratoPor100g = carbo, GrasaPor100g = grasa,
            PorcionTipicaG = 150, PorcionMinimaG = min, PorcionMaximaG = max
        };

        private static readonly Alimento Pollo =
            Alimento("pechuga-de-pollo", "proteina", "proteina-magra", 22.5, 0, 2.62, 80, 250);
        private static readonly Alimento Pavo =
            Alimento("pavo-pechuga", "proteina", "proteina-magra", 23.7, 0.14, 1.48, 80, 250);
        private static readonly Alimento AtunEnAgua =
            Alimento("atun-en-agua", "proteina", "proteina-magra", 25.5, 0, 0.82, 60, 180);

        private static PorcionAlimento Porcion(Alimento a, double gramos) =>
            new() { Alimento = a, Gramos = gramos };

        [Fact]
        public void ElSustitutoIgualaElMacroPrincipal()
        {
            // 150 g de pollo = 33.75 g de proteína. El pavo tiene 23.7 g/100 g,
            // así que hacen falta 142.4 g → redondeado a 140.
            var sustitutos = CalculadorEquivalencias.Para(Porcion(Pollo, 150), new[] { Pavo });

            var pavo = Assert.Single(sustitutos);
            Assert.Equal("pavo-pechuga", pavo.Alimento.Slug);
            // El redondeo a múltiplos de 5 g mueve la equivalencia un par de gramos:
            // se busca "aporta lo mismo", no una igualdad al decimal.
            Assert.InRange(pavo.Macros.ProteinaG, 32, 35.5);
        }

        [Fact]
        public void UnAlimentoNoSeSustituyeASiMismo()
        {
            var sustitutos = CalculadorEquivalencias.Para(Porcion(Pollo, 150), new[] { Pollo, Pavo });

            Assert.DoesNotContain(sustitutos, s => s.Alimento.Slug == "pechuga-de-pollo");
        }

        [Fact]
        public void SeDescartaElSustitutoQueExigiriaUnaPorcionAbsurda()
        {
            // Sustituir por algo con poquísima proteína pediría una porción enorme.
            var lechuga = Alimento("lechuga", "verdura", "verdura", 1.0, 2.0, 0.2, 30, 200);

            var sustitutos = CalculadorEquivalencias.Para(Porcion(Pollo, 150),
                new[] { lechuga });

            // 33.75 g de proteína a 1 g/100 g = 3375 g: muy por encima del máximo.
            Assert.Empty(sustitutos);
        }

        [Fact]
        public void SeDescartaElCandidatoQueNoAportaElMacro()
        {
            // Un alimento sin nada de proteína no puede sustituir a una proteína:
            // la regla de tres dividiría por cero.
            var aceite = Alimento("aceite", "grasa", "grasa", 0, 0, 100, 5, 30);

            var sustitutos = CalculadorEquivalencias.Para(Porcion(Pollo, 150), new[] { aceite });

            Assert.Empty(sustitutos);
        }

        [Fact]
        public void LosSustitutosSeOrdenanPorCercaniaCalorica()
        {
            var sustitutos = CalculadorEquivalencias.Para(
                Porcion(Pollo, 150), new[] { Pavo, AtunEnAgua });

            var caloriasPollo = Porcion(Pollo, 150).Macros.Calorias;
            var desvios = sustitutos
                .Select(s => Math.Abs(s.Macros.Calorias - caloriasPollo))
                .ToList();

            // Vienen del más parecido al menos.
            Assert.True(desvios.SequenceEqual(desvios.OrderBy(d => d)));
        }

        [Fact]
        public void SeRespetaElTopeDeCuantosSustitutos()
        {
            var muchos = Enumerable.Range(1, 10)
                .Select(i => Alimento($"prot-{i}", "proteina", "proteina-magra", 20 + i, 0, 2))
                .ToArray();

            var sustitutos = CalculadorEquivalencias.Para(Porcion(Pollo, 150), muchos, cuantos: 3);

            Assert.Equal(3, sustitutos.Count);
        }

        [Fact]
        public void SinCandidatos_NoHaySustitutos()
        {
            var sustitutos = CalculadorEquivalencias.Para(Porcion(Pollo, 150), Array.Empty<Alimento>());

            Assert.Empty(sustitutos);
        }

        [Fact]
        public void ParaUnCereal_SeIgualaElCarbohidrato()
        {
            var arroz = Alimento("arroz", "carbohidrato", "cereal", 2.7, 28.2, 0.3, 80, 300);
            var quinoa = Alimento("quinoa", "carbohidrato", "cereal", 4.4, 21.3, 1.9, 80, 280);

            // 150 g de arroz = 42.3 g de carbohidrato. La quinoa tiene 21.3 g/100 g.
            var sustitutos = CalculadorEquivalencias.Para(Porcion(arroz, 150), new[] { quinoa });

            var q = Assert.Single(sustitutos);
            Assert.InRange(q.Macros.CarbohidratoG, 40, 44);
        }

        [Fact]
        public void PorcionNula_Lanza()
        {
            Assert.Throws<ArgumentNullException>(
                () => CalculadorEquivalencias.Para(null!, new[] { Pavo }));
        }
    }
}
