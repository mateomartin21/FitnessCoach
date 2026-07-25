using System.Text.Json;
using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Data
{
    /// <summary>
    /// Corre el generador contra el catálogo real de la semilla, no contra el falso.
    ///
    /// El catálogo de pruebas tiene una decena de alimentos elegidos para que todo
    /// encaje; el real tiene 67 con densidades muy distintas. Un reparto que funciona
    /// con el primero puede dar porciones absurdas con el segundo, y eso es lo que
    /// vería el usuario.
    /// </summary>
    public class PlanConCatalogoRealTests
    {
        private static readonly IReadOnlyList<Alimento> CatalogoReal = Cargar();

        private static IReadOnlyList<Alimento> Cargar()
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "Data", "catalogo-alimentos.json");
            var alimentos = JsonSerializer.Deserialize<List<Alimento>>(
                File.ReadAllText(ruta), new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(alimentos);
            return alimentos!;
        }

        private static GeneradorAlimentacionService Generador() =>
            new(new RepositorioAlimentosFalso(CatalogoReal.ToArray()), new CalculadorCaloricoService());

        private static UsuarioPerfil Usuario(ObjetivoFitness objetivo, double peso, int estatura, int edad, int id = 1) => new()
        {
            Id = id,
            Nombre = "Persona",
            Edad = edad,
            EstaturaCm = estatura,
            PesoKg = peso,
            ObjetivoActual = objetivo
        };

        /// <summary>Perfiles bien distintos entre sí, para no probar un solo caso cómodo.</summary>
        public static TheoryData<string, ObjetivoFitness, double, int, int> Perfiles => new()
        {
            { "mujer liviana en déficit", new ObjetivoPerderPeso(), 52, 158, 26 },
            { "hombre pesado en volumen", new ObjetivoGanarMusculo(), 95, 188, 22 },
            { "persona mayor en recomposición", new ObjetivoRecomposicion(), 78, 170, 58 },
            { "persona muy pesada en déficit", new ObjetivoPerderPeso(), 120, 175, 40 },
            { "persona muy liviana en volumen", new ObjetivoGanarMusculo(), 45, 152, 19 },
        };

        [Theory]
        [MemberData(nameof(Perfiles))]
        public void ElPlanSeAcercaAlObjetivoCalorico(
            string caso, ObjetivoFitness objetivo, double peso, int estatura, int edad)
        {
            var plan = Generador().GenerarPlanPara(Usuario(objetivo, peso, estatura, edad));

            Assert.InRange(plan.DesvioCaloricoPorcentaje, -20, 20);
        }

        [Theory]
        [MemberData(nameof(Perfiles))]
        public void ElPlanCubreLaProteinaObjetivo(
            string caso, ObjetivoFitness objetivo, double peso, int estatura, int edad)
        {
            var plan = Generador().GenerarPlanPara(Usuario(objetivo, peso, estatura, edad));

            // La proteína es el macro que no puede quedarse corto: es lo que protege
            // el músculo, sobre todo en déficit.
            Assert.True(plan.ProteinaTotalG >= plan.Objetivos.ProteinaG * 0.80,
                $"{caso}: el plan aporta {plan.ProteinaTotalG} g sobre un objetivo de {plan.Objetivos.ProteinaG} g.");
        }

        [Theory]
        [MemberData(nameof(Perfiles))]
        public void NingunaPorcionEsAbsurda(
            string caso, ObjetivoFitness objetivo, double peso, int estatura, int edad)
        {
            var plan = Generador().GenerarPlanPara(Usuario(objetivo, peso, estatura, edad));

            foreach (var porcion in plan.Comidas.SelectMany(c => c.Porciones))
            {
                Assert.InRange(porcion.Gramos,
                    porcion.Alimento.PorcionMinimaG,
                    porcion.Alimento.PorcionMaximaG);
            }
        }

        [Theory]
        [MemberData(nameof(Perfiles))]
        public void TodasLasComidasTienenAlgoDeComer(
            string caso, ObjetivoFitness objetivo, double peso, int estatura, int edad)
        {
            var plan = Generador().GenerarPlanPara(Usuario(objetivo, peso, estatura, edad));

            Assert.NotEmpty(plan.Comidas);
            Assert.All(plan.Comidas, c =>
            {
                Assert.NotEmpty(c.Porciones);
                Assert.True(c.Calorias > 0, $"{caso}: '{c.NombreComida}' no aporta calorías.");
            });
        }

        [Fact]
        public void ConElCatalogoReal_NoHaceFaltaRepetirAlimentos()
        {
            // El catálogo real tiene alimentos de sobra: si aparece una repetición,
            // es que a alguna categoría le faltan opciones para la estructura del plan.
            foreach (var objetivo in new ObjetivoFitness[]
                     { new ObjetivoPerderPeso(), new ObjetivoGanarMusculo(), new ObjetivoRecomposicion() })
            {
                var plan = Generador().GenerarPlanPara(Usuario(objetivo, 75, 175, 30));

                var slugs = plan.Comidas.SelectMany(c => c.Porciones)
                    .Select(p => p.Alimento.Slug).ToList();

                Assert.Equal(slugs.Count, slugs.Distinct().Count());
            }
        }

        [Fact]
        public void DistintosUsuariosConElMismoObjetivo_RecibenPlanesDistintos()
        {
            var planes = Enumerable.Range(1, 5)
                .Select(id => Generador().GenerarPlanPara(
                    Usuario(new ObjetivoPerderPeso(), 75, 175, 30, id)))
                .Select(p => string.Join("|", p.Comidas.SelectMany(c => c.Alimentos)))
                .ToList();

            Assert.Equal(planes.Count, planes.Distinct().Count());
        }

        [Fact]
        public void UnVegetarianoConAlergia_RecibeUnPlanCompletoSinNadaExcluido()
        {
            // La prueba que cierra la fase: preferencias reales sobre el catálogo real.
            var usuario = Usuario(new ObjetivoRecomposicion(), 70, 172, 28);
            usuario.Preferencias.DietasSeguidas.Add("vegetariano");
            usuario.Preferencias.AlimentosExcluidos.Add("mani");          // alergia
            usuario.Preferencias.AlimentosExcluidos.Add("mantequilla-de-mani");

            var plan = Generador().GenerarPlanPara(usuario);

            var todos = plan.Comidas.SelectMany(c => c.Porciones)
                .SelectMany(p => new[] { p.Alimento }.Concat(p.Sustitutos.Select(s => s.Alimento)))
                .ToList();

            // Nada excluido, ni como comida ni como sustituto.
            Assert.All(todos, a => Assert.True(a.Cumple("vegetariano"), $"'{a.Nombre}' no es vegetariano."));
            Assert.DoesNotContain(todos, a => a.Slug is "mani" or "mantequilla-de-mani");

            // Y sigue siendo un plan que sirve: comidas completas y proteína cubierta.
            Assert.All(plan.Comidas, c => Assert.NotEmpty(c.Porciones));
            Assert.True(plan.ProteinaTotalG >= plan.Objetivos.ProteinaG * 0.75,
                $"El plan vegetariano aporta {plan.ProteinaTotalG} g sobre {plan.Objetivos.ProteinaG} g objetivo.");
        }

        [Fact]
        public void UnVeganoRecibeUnPlanSinProductoAnimal()
        {
            var usuario = Usuario(new ObjetivoPerderPeso(), 68, 170, 32);
            usuario.Preferencias.DietasSeguidas.Add("vegano");

            var plan = Generador().GenerarPlanPara(usuario);

            var todos = plan.Comidas.SelectMany(c => c.Porciones)
                .SelectMany(p => new[] { p.Alimento }.Concat(p.Sustitutos.Select(s => s.Alimento)))
                .ToList();

            Assert.NotEmpty(plan.Comidas);
            Assert.All(todos, a => Assert.True(a.Cumple("vegano"), $"'{a.Nombre}' no es vegano."));
        }
    }
}
