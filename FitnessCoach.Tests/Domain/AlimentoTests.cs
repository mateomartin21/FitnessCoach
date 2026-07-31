using FitnessCoach.Domain.Models.Alimentacion;
using Xunit;

namespace FitnessCoach.Tests.Domain
{
    public class AlimentoTests
    {
        private static Alimento PechugaDePollo() => new()
        {
            Slug = "pechuga-de-pollo",
            Nombre = "Pechuga de pollo",
            Categoria = "proteina",
            GrupoIntercambio = "proteina-magra",
            ProteinaPor100g = 22.5,
            CarbohidratoPor100g = 0,
            GrasaPor100g = 2.62,
            EtiquetasDieta = new List<string> { "sin-gluten", "sin-lactosa" }
        };

        [Fact]
        public void LasCaloriasSalenDeLosMacros_ConLosFactoresDeAtwater()
        {
            var pollo = PechugaDePollo();

            // 22.5×4 + 0×4 + 2.62×9 = 90 + 23.58 = 113.58
            Assert.Equal(113.58, pollo.CaloriasPor100g, precision: 2);
        }

        [Fact]
        public void LasCaloriasSiguenALosMacros_NoPuedenQuedarDesincronizadas()
        {
            // Es la razón de que no sean una columna: al cambiar un macro, el total
            // se recalcula solo. Guardado aparte podría mentir.
            var pollo = PechugaDePollo();
            var antes = pollo.CaloriasPor100g;

            pollo.GrasaPor100g += 10;

            Assert.Equal(antes + 90, pollo.CaloriasPor100g, precision: 2);
        }

        [Fact]
        public void MacrosPara_EscalaProporcionalmente()
        {
            var macros = PechugaDePollo().MacrosPara(200);

            Assert.Equal(200, macros.Gramos);
            Assert.Equal(45.0, macros.ProteinaG, precision: 2);
            Assert.Equal(5.24, macros.GrasaG, precision: 2);
        }

        [Fact]
        public void MacrosPara100g_DevuelveLosValoresDeLaTabla()
        {
            var pollo = PechugaDePollo();
            var macros = pollo.MacrosPara(100);

            Assert.Equal(pollo.ProteinaPor100g, macros.ProteinaG, precision: 4);
            Assert.Equal(pollo.CaloriasPor100g, macros.Calorias, precision: 4);
        }

        [Fact]
        public void MacrosPara_EnCeroGramos_NoAportaNada()
        {
            var macros = PechugaDePollo().MacrosPara(0);

            Assert.Equal(0, macros.Calorias);
            Assert.Equal(0, macros.ProteinaG);
        }

        [Fact]
        public void MacrosPara_ConGramosNegativos_Lanza()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PechugaDePollo().MacrosPara(-50));
        }

        [Fact]
        public void LasCaloriasDeLaPorcionSiguenSiendoCoherentesConSusMacros()
        {
            var macros = PechugaDePollo().MacrosPara(175);

            var recalculadas = macros.ProteinaG * ObjetivoMacros.KcalPorGramoProteina
                             + macros.CarbohidratoG * ObjetivoMacros.KcalPorGramoCarbohidrato
                             + macros.GrasaG * ObjetivoMacros.KcalPorGramoGrasa;

            Assert.Equal(recalculadas, macros.Calorias, precision: 6);
        }

        [Theory]
        [InlineData("sin-gluten", true)]
        [InlineData("SIN-GLUTEN", true)]   // la comparación no distingue mayúsculas
        [InlineData("vegano", false)]
        public void Cumple_ReconoceLasEtiquetasDeDieta(string etiqueta, bool esperado)
        {
            Assert.Equal(esperado, PechugaDePollo().Cumple(etiqueta));
        }

        [Theory]
        [InlineData("proteina", 22.5, 0, 2.62, "proteina")]
        [InlineData("carbohidrato", 2.7, 28.2, 0.3, "carbohidrato")]
        [InlineData("grasa", 0, 0, 100, "grasa")]
        [InlineData("fruta", 1.1, 22.8, 0.3, "carbohidrato")]
        [InlineData("verdura", 2.8, 6.6, 0.4, "carbohidrato")]
        public void MacroPrincipal_LoDefineLaCategoria(
            string categoria, double prot, double carbo, double grasa, string esperado)
        {
            var alimento = new Alimento
            {
                Slug = "x", Categoria = categoria,
                ProteinaPor100g = prot, CarbohidratoPor100g = carbo, GrasaPor100g = grasa
            };

            Assert.Equal(esperado, alimento.MacroPrincipal);
        }

        [Fact]
        public void MacroPrincipal_ParaUnLacteo_DependeDeSusMacros()
        {
            // El yogur griego es proteína; la ricotta, con más grasa, es grasa.
            var yogur = new Alimento { Categoria = "lacteo", ProteinaPor100g = 9.5, GrasaPor100g = 0.24 };
            var ricotta = new Alimento { Categoria = "lacteo", ProteinaPor100g = 11.4, GrasaPor100g = 7.91 };

            Assert.Equal("proteina", yogur.MacroPrincipal);
            Assert.Equal("grasa", ricotta.MacroPrincipal);
        }

        [Fact]
        public void GramosDelMacroPrincipal_DevuelveElMacroQueCorresponde()
        {
            var pollo = PechugaDePollo();   // proteína

            // 200 g × 22.5 g/100 g = 45 g de proteína
            Assert.Equal(45.0, pollo.GramosDelMacroPrincipal(200), precision: 2);
        }

        [Fact]
        public void SinImagen_NoHayAtribucionQueMostrar()
        {
            var alimento = PechugaDePollo();
            alimento.UrlImagen = null;

            Assert.Null(alimento.AtribucionImagen);
        }

        [Fact]
        public void ConImagen_LaAtribucionIncluyeAutorYLicencia()
        {
            var alimento = PechugaDePollo();
            alimento.UrlImagen = "https://upload.wikimedia.org/foto.jpg";
            alimento.AutorImagen = "Fulano";
            alimento.LicenciaImagen = "CC BY-SA 3.0";

            Assert.Contains("Fulano", alimento.AtribucionImagen);
            Assert.Contains("CC BY-SA 3.0", alimento.AtribucionImagen);
        }
    }
}
