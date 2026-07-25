using FitnessCoach.Domain.Models.Alimentacion;
using Xunit;

namespace FitnessCoach.Tests.Domain
{
    public class PreferenciasAlimentariasTests
    {
        private static Alimento Alimento(string slug, params string[] etiquetas) => new()
        {
            Slug = slug, Nombre = slug, Categoria = "proteina",
            EtiquetasDieta = etiquetas.ToList()
        };

        [Fact]
        public void SinRestricciones_DejaPasarTodo()
        {
            var prefs = new PreferenciasAlimentarias();

            Assert.True(prefs.SinRestricciones);
            Assert.True(prefs.Permite(Alimento("pollo")));
        }

        [Fact]
        public void UnaDietaSeguida_ExigeQueElAlimentoLaCumpla()
        {
            var prefs = new PreferenciasAlimentarias
            {
                DietasSeguidas = new List<string> { "vegetariano" }
            };

            Assert.False(prefs.Permite(Alimento("pollo")));                       // sin la etiqueta
            Assert.True(prefs.Permite(Alimento("tofu", "vegetariano", "vegano")));  // la cumple
        }

        [Fact]
        public void VariasDietas_SeExigenTodas()
        {
            var prefs = new PreferenciasAlimentarias
            {
                DietasSeguidas = new List<string> { "vegano", "sin-gluten" }
            };

            // Cumple una pero no la otra: no pasa.
            Assert.False(prefs.Permite(Alimento("seitan", "vegano")));
            Assert.True(prefs.Permite(Alimento("lentejas", "vegano", "sin-gluten")));
        }

        [Fact]
        public void UnAlimentoExcluido_NuncaPasa_AunqueCumplaLasDietas()
        {
            var prefs = new PreferenciasAlimentarias
            {
                DietasSeguidas = new List<string> { "vegano" },
                AlimentosExcluidos = new List<string> { "mani" }
            };

            // El maní es vegano, pero está vetado (alergia): manda el veto.
            Assert.False(prefs.Permite(Alimento("mani", "vegano", "sin-gluten")));
        }

        [Fact]
        public void ElVetoNoDistingueMayusculas()
        {
            var prefs = new PreferenciasAlimentarias
            {
                AlimentosExcluidos = new List<string> { "MANI" }
            };

            Assert.False(prefs.Permite(Alimento("mani")));
        }

        [Fact]
        public void ConAlgunaRestriccion_NoEstaSinRestricciones()
        {
            Assert.False(new PreferenciasAlimentarias
            {
                DietasSeguidas = new List<string> { "vegano" }
            }.SinRestricciones);

            Assert.False(new PreferenciasAlimentarias
            {
                AlimentosExcluidos = new List<string> { "mani" }
            }.SinRestricciones);
        }

        [Fact]
        public void AlimentoNulo_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => new PreferenciasAlimentarias().Permite(null!));
        }
    }
}
