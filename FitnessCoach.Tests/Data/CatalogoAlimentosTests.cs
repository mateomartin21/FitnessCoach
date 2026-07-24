using System.Text.Json;
using FitnessCoach.Domain.Models.Alimentacion;
using Xunit;

namespace FitnessCoach.Tests.Data
{
    /// <summary>
    /// Valida el JSON de semilla del catálogo de alimentos.
    ///
    /// Existe por una razón concreta: al importar desde USDA, la búsqueda aproximada
    /// calzó "Bananas, raw" contra "Beef, tripe, raw" — mondongo con 12 g de proteína
    /// y 0 de carbohidratos en lugar de una banana. Ninguna otra prueba lo habría
    /// visto, porque el código era correcto; los datos no. Estas comprobaciones son
    /// la red para cuando el catálogo se vuelva a importar.
    /// </summary>
    public class CatalogoAlimentosTests
    {
        private static readonly List<Alimento> Catalogo = Cargar();

        private static List<Alimento> Cargar()
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "Data", "catalogo-alimentos.json");
            Assert.True(File.Exists(ruta), $"No se encontró la semilla del catálogo en {ruta}.");

            var alimentos = JsonSerializer.Deserialize<List<Alimento>>(
                File.ReadAllText(ruta), new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(alimentos);
            return alimentos!;
        }

        [Fact]
        public void ElCatalogoTieneAlimentosSuficientesParaArmarUnPlan()
        {
            Assert.InRange(Catalogo.Count, 50, 500);
        }

        [Fact]
        public void LosSlugsSonUnicos()
        {
            var repetidos = Catalogo.GroupBy(a => a.Slug)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            Assert.Empty(repetidos);
        }

        [Fact]
        public void NingunAlimentoSuperaLos100GramosDeMacrosPorCada100Gramos()
        {
            // Imposible por definición: sería más masa de macros que de alimento.
            var imposibles = Catalogo
                .Where(a => a.ProteinaPor100g + a.CarbohidratoPor100g + a.GrasaPor100g > 100.5)
                .Select(a => a.Slug)
                .ToList();

            Assert.Empty(imposibles);
        }

        [Fact]
        public void TodoAlimentoAportaAlgunMacro()
        {
            // Todo en cero delata un nutriente que no se pudo leer del origen.
            var vacios = Catalogo
                .Where(a => a.ProteinaPor100g + a.CarbohidratoPor100g + a.GrasaPor100g == 0)
                .Select(a => a.Slug)
                .ToList();

            Assert.Empty(vacios);
        }

        [Theory]
        [InlineData("fruta", 5.0)]
        [InlineData("verdura", 6.0)]
        public void LasFrutasYVerdurasNoTienenProteinaDeCarne(string categoria, double maximo)
        {
            // Es la comprobación que atrapa un calce cruzado como banana -> mondongo.
            var sospechosos = Catalogo
                .Where(a => a.Categoria == categoria && a.ProteinaPor100g > maximo)
                .Select(a => $"{a.Slug} ({a.ProteinaPor100g} g de proteína)")
                .ToList();

            Assert.Empty(sospechosos);
        }

        [Fact]
        public void LasProteinasAportanProteina()
        {
            var flojos = Catalogo
                .Where(a => a.Categoria == "proteina" && a.ProteinaPor100g < 8)
                .Select(a => $"{a.Slug} ({a.ProteinaPor100g} g)")
                .ToList();

            Assert.Empty(flojos);
        }

        [Fact]
        public void NingunAlimentoSuperaLas900KcalPor100g()
        {
            // El techo teórico es la grasa pura: 100 g × 9 kcal.
            var imposibles = Catalogo
                .Where(a => a.CaloriasPor100g > 900.5)
                .Select(a => $"{a.Slug} ({a.CaloriasPor100g:0} kcal)")
                .ToList();

            Assert.Empty(imposibles);
        }

        [Fact]
        public void LasPorcionesEstanOrdenadas()
        {
            var desordenadas = Catalogo
                .Where(a => !(a.PorcionMinimaG <= a.PorcionTipicaG && a.PorcionTipicaG <= a.PorcionMaximaG))
                .Select(a => $"{a.Slug} ({a.PorcionMinimaG}/{a.PorcionTipicaG}/{a.PorcionMaximaG})")
                .ToList();

            Assert.Empty(desordenadas);
        }

        [Fact]
        public void TodaImagenTraeSuAtribucion()
        {
            // Las fotos son CC BY-SA: sin autor y licencia no se pueden mostrar.
            var sinAtribuir = Catalogo
                .Where(a => a.UrlImagen is not null
                            && (string.IsNullOrWhiteSpace(a.AutorImagen)
                                || string.IsNullOrWhiteSpace(a.LicenciaImagen)))
                .Select(a => a.Slug)
                .ToList();

            Assert.Empty(sinAtribuir);
        }

        [Fact]
        public void TodoAlimentoTieneNombreCategoriaYGrupoDeIntercambio()
        {
            var incompletos = Catalogo
                .Where(a => string.IsNullOrWhiteSpace(a.Nombre)
                            || string.IsNullOrWhiteSpace(a.Categoria)
                            || string.IsNullOrWhiteSpace(a.GrupoIntercambio))
                .Select(a => a.Slug)
                .ToList();

            Assert.Empty(incompletos);
        }

        [Fact]
        public void CadaGrupoDeIntercambioTieneConQueSustituir()
        {
            // Un grupo con un solo alimento no ofrece alternativas: o sobra el grupo,
            // o falta el alimento que lo acompañe.
            var solitarios = Catalogo
                .GroupBy(a => a.GrupoIntercambio)
                .Where(g => g.Count() < 2)
                .Select(g => $"{g.Key}: {g.Single().Slug}")
                .ToList();

            Assert.Empty(solitarios);
        }
    }
}
