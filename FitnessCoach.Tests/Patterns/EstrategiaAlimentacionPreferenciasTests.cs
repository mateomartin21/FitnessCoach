using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Patterns.Strategy.Alimentacion;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Patterns
{
    public class EstrategiaAlimentacionPreferenciasTests
    {
        private static readonly ObjetivoMacros Macros = new(2000, 154, 56, 173);

        private static PlanAlimentacion PlanCon(PreferenciasAlimentarias prefs) =>
            new AlimentacionPerderPeso(RepositorioAlimentosFalso.ConCatalogoDePrueba(), 0, prefs)
                .GenerarPlan(Macros);

        private static IEnumerable<Alimento> TodosLosAlimentos(PlanAlimentacion plan) =>
            plan.Comidas.SelectMany(c => c.Porciones)
                .SelectMany(p => new[] { p.Alimento }.Concat(p.Sustitutos.Select(s => s.Alimento)));

        [Fact]
        public void UnVegetariano_NuncaVeCarneNiPescado_NiComoComidaNiComoSustituto()
        {
            var prefs = new PreferenciasAlimentarias
            {
                DietasSeguidas = new List<string> { "vegetariano" }
            };

            var plan = PlanCon(prefs);

            Assert.All(TodosLosAlimentos(plan),
                a => Assert.True(a.Cumple("vegetariano"),
                    $"'{a.Nombre}' no es vegetariano y apareció en el plan de un vegetariano."));
        }

        [Fact]
        public void UnAlimentoExcluido_NoApareceEnNingunLado()
        {
            var prefs = new PreferenciasAlimentarias
            {
                AlimentosExcluidos = new List<string> { "pechuga-de-pollo" }
            };

            var plan = PlanCon(prefs);

            Assert.DoesNotContain(TodosLosAlimentos(plan), a => a.Slug == "pechuga-de-pollo");
        }

        [Fact]
        public void ConUnaRestriccionRazonable_ElPlanSigueTeniendoComidas()
        {
            var prefs = new PreferenciasAlimentarias
            {
                DietasSeguidas = new List<string> { "vegetariano" }
            };

            var plan = PlanCon(prefs);

            Assert.NotEmpty(plan.Comidas);
            Assert.All(plan.Comidas, c => Assert.NotEmpty(c.Porciones));
        }

        [Fact]
        public void SinPreferencias_SeComportaComoAntes()
        {
            // Pasar null equivale a "sin restricciones": el plan incluye proteína animal.
            var plan = new AlimentacionPerderPeso(RepositorioAlimentosFalso.ConCatalogoDePrueba(), 0, null)
                .GenerarPlan(Macros);

            Assert.Contains(TodosLosAlimentos(plan), a => !a.Cumple("vegetariano"));
        }

        [Fact]
        public void ElVeganoNoRecibeLacteos()
        {
            // En el catálogo de prueba los lácteos son vegetarianos pero no veganos.
            var prefs = new PreferenciasAlimentarias
            {
                DietasSeguidas = new List<string> { "vegano" }
            };

            var plan = PlanCon(prefs);

            Assert.DoesNotContain(plan.Comidas.SelectMany(c => c.Porciones),
                p => p.Alimento.Categoria == "lacteo");
        }
    }
}
