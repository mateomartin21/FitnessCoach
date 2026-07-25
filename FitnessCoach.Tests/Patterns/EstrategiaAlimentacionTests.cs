using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Patterns.Strategy.Alimentacion;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Patterns
{
    public class EstrategiaAlimentacionTests
    {
        private static readonly ObjetivoMacros Macros = new(2000, 154, 56, 173);

        private static PlanAlimentacion PlanDePrueba(ObjetivoMacros? macros = null) =>
            new AlimentacionPerderPeso(RepositorioAlimentosFalso.ConCatalogoDePrueba())
                .GenerarPlan(macros ?? Macros);

        [Fact]
        public void ElPlanTieneComidasConAlimentosDelCatalogo()
        {
            var plan = PlanDePrueba();

            Assert.NotEmpty(plan.Comidas);
            Assert.All(plan.Comidas, c => Assert.NotEmpty(c.Porciones));
        }

        [Fact]
        public void LosMacrosDeLaComidaSalenDeSusPorciones()
        {
            // No se escriben a mano: si no coincidieran, el plan estaría mintiendo
            // sobre los alimentos que él mismo lista.
            var comida = PlanDePrueba().Comidas.First();

            var proteinaSumada = comida.Porciones.Sum(p => p.Macros.ProteinaG);

            Assert.Equal((int)Math.Round(proteinaSumada), comida.Proteinas);
        }

        [Fact]
        public void ElPlanSeAcercaAlObjetivoCalorico()
        {
            var plan = PlanDePrueba();

            // No se exige exactitud: las porciones se redondean y se acotan a cantidades
            // razonables. Pero un desvío grande significaría que el reparto no funciona.
            Assert.InRange(plan.DesvioCaloricoPorcentaje, -25, 25);
        }

        [Fact]
        public void ElPlanCubreLaMayorParteDeLaProteinaObjetivo()
        {
            var plan = PlanDePrueba();

            // La proteína es el macro que no se puede quedar corto: es lo que protege
            // el músculo. Se acepta un margen, pero no quedarse a la mitad.
            Assert.True(plan.ProteinaTotalG >= Macros.ProteinaG * 0.75,
                $"El plan aporta {plan.ProteinaTotalG} g de proteína sobre un objetivo de {Macros.ProteinaG} g.");
        }

        [Fact]
        public void MasCalorias_DanMasComida()
        {
            var chico = PlanDePrueba(new ObjetivoMacros(1500, 120, 42, 116));
            var grande = PlanDePrueba(new ObjetivoMacros(3000, 180, 83, 331));

            Assert.True(grande.CaloriasTotales > chico.CaloriasTotales);
        }

        [Fact]
        public void NoSeRepiteUnAlimentoMientrasQuedenAlternativas()
        {
            // Cinco comidas de pollo con arroz cumplen los macros y no las sigue nadie.
            // Repetir solo se permite cuando la categoría se quedó sin alimentos nuevos:
            // un plan que se queda corto de proteína es peor que uno que repite el pollo.
            var catalogo = RepositorioAlimentosFalso.ConCatalogoDePrueba();
            var plan = new AlimentacionPerderPeso(catalogo).GenerarPlan(Macros);

            var usados = plan.Comidas.SelectMany(c => c.Porciones).Select(p => p.Alimento).ToList();

            foreach (var porCategoria in usados.GroupBy(a => a.Categoria))
            {
                var disponibles = catalogo.ObtenerPorCategoria(porCategoria.Key).Count;
                var distintos = porCategoria.Select(a => a.Slug).Distinct().Count();

                // Se agotó el catálogo de esa categoría, o no hubo ninguna repetición.
                Assert.True(distintos == disponibles || distintos == porCategoria.Count(),
                    $"En '{porCategoria.Key}' se usaron {porCategoria.Count()} porciones con solo " +
                    $"{distintos} alimentos distintos, habiendo {disponibles} disponibles.");
            }
        }

        [Fact]
        public void UnAlimentoNoSeRepiteDentroDeLaMismaComida()
        {
            var plan = PlanDePrueba();

            foreach (var comida in plan.Comidas)
            {
                var slugs = comida.Porciones.Select(p => p.Alimento.Slug).ToList();
                Assert.Equal(slugs.Count, slugs.Distinct().Count());
            }
        }

        [Fact]
        public void LasPorcionesRespetanLosLimitesDelAlimento()
        {
            // Sin esto, cuadrar los macros podría pedir 12 g de arroz o 900 g de brócoli.
            var plan = PlanDePrueba(new ObjetivoMacros(4000, 300, 111, 425));

            foreach (var porcion in plan.Comidas.SelectMany(c => c.Porciones))
            {
                Assert.InRange(porcion.Gramos,
                    porcion.Alimento.PorcionMinimaG,
                    porcion.Alimento.PorcionMaximaG);
            }
        }

        [Fact]
        public void LasPorcionesSeRedondeanAMultiplosDeCinco()
        {
            // Nadie pesa 137 g de arroz.
            var plan = PlanDePrueba();

            Assert.All(plan.Comidas.SelectMany(c => c.Porciones),
                p => Assert.Equal(0, p.Gramos % 5));
        }

        [Fact]
        public void ConElCatalogoVacio_ElPlanNoTraeComidasHuecas()
        {
            var plan = new AlimentacionPerderPeso(new RepositorioAlimentosFalso()).GenerarPlan(Macros);

            // Mejor un plan vacío que uno con comidas sin nada adentro.
            Assert.Empty(plan.Comidas);
            Assert.Equal(0, plan.CaloriasTotales);
        }

        [Fact]
        public void SinCatalogo_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => new AlimentacionPerderPeso(null!));
        }

        [Fact]
        public void ElPlanLlevaLosObjetivosQueRecibio()
        {
            var plan = PlanDePrueba();

            Assert.Equal(Macros, plan.Objetivos);
        }

        [Fact]
        public void CadaEstrategiaTieneSuPropiaEstructuraDeComidas()
        {
            var catalogo = RepositorioAlimentosFalso.ConCatalogoDePrueba();

            var volumen = new AlimentacionGanarMusculo(catalogo).GenerarPlan(Macros);
            var recomposicion = new AlimentacionRecomposicion(catalogo).GenerarPlan(Macros);

            // Volumen reparte en seis comidas; recomposición en cuatro.
            Assert.True(volumen.Comidas.Count > recomposicion.Comidas.Count);
        }

        [Fact]
        public void ElDesayunoNoTraeAlimentosDeAlmuerzo()
        {
            // Cuadrar los macros no alcanza: 115 g de tempeh con pasta a las siete de
            // la mañana es correcto en números y no lo desayuna nadie.
            var plan = PlanDePrueba();
            var desayuno = plan.Comidas.First(c => c.NombreComida == "Desayuno");

            Assert.All(desayuno.Porciones, p =>
                Assert.True(p.Alimento.VaEn("desayuno"),
                    $"'{p.Alimento.Nombre}' no es un alimento de desayuno."));
        }

        [Fact]
        public void CadaComidaRespetaSuMomentoDelDia()
        {
            var catalogo = RepositorioAlimentosFalso.ConCatalogoDePrueba();
            var plan = new AlimentacionRecomposicion(catalogo).GenerarPlan(Macros);

            var momentoDe = new Dictionary<string, string>
            {
                ["Desayuno"] = "desayuno",
                ["Almuerzo"] = "principal",
                ["Merienda"] = "snack",
                ["Cena"] = "principal"
            };

            foreach (var comida in plan.Comidas)
            {
                var momento = momentoDe[comida.NombreComida];
                Assert.All(comida.Porciones, p =>
                    Assert.True(p.Alimento.VaEn(momento),
                        $"'{p.Alimento.Nombre}' no va en '{comida.NombreComida}'."));
            }
        }

        [Fact]
        public void SiNingunAlimentoDeLaCategoriaVaEnEseMomento_IgualSeSirveAlgo()
        {
            // Un desayuno raro es mejor que un desayuno vacío: si el catálogo no tiene
            // nada marcado para ese momento, se vuelve al catálogo entero.
            var soloParaCenar = new RepositorioAlimentosFalso(new Alimento
            {
                Slug = "pescado", Nombre = "Pescado", Categoria = "proteina",
                GrupoIntercambio = "pescado-blanco",
                ProteinaPor100g = 20, CarbohidratoPor100g = 0, GrasaPor100g = 2,
                PorcionTipicaG = 150, PorcionMinimaG = 80, PorcionMaximaG = 250,
                MomentosAptos = new List<string> { "principal" }
            });

            var plan = new AlimentacionPerderPeso(soloParaCenar).GenerarPlan(Macros);
            var desayuno = plan.Comidas.FirstOrDefault(c => c.NombreComida == "Desayuno");

            Assert.NotNull(desayuno);
            Assert.NotEmpty(desayuno!.Porciones);
        }

        [Fact]
        public void LasPorcionesTraenSustitutosDelMismoGrupoDeIntercambio()
        {
            var plan = PlanDePrueba();

            // Al menos alguna porción debe ofrecer alternativas; y toda alternativa
            // tiene que ser del mismo grupo de intercambio que la original.
            var conSustitutos = plan.Comidas
                .SelectMany(c => c.Porciones)
                .Where(p => p.Sustitutos.Count > 0)
                .ToList();

            Assert.NotEmpty(conSustitutos);
            foreach (var porcion in conSustitutos)
            {
                Assert.All(porcion.Sustitutos, s =>
                    Assert.Equal(porcion.Alimento.GrupoIntercambio, s.Alimento.GrupoIntercambio));
            }
        }

        [Fact]
        public void UnSustitutoNoEsElMismoAlimentoDeLaPorcion()
        {
            var plan = PlanDePrueba();

            foreach (var porcion in plan.Comidas.SelectMany(c => c.Porciones))
            {
                Assert.DoesNotContain(porcion.Sustitutos,
                    s => s.Alimento.Slug == porcion.Alimento.Slug);
            }
        }

        [Fact]
        public void LosSustitutosRespetanElMomentoDeLaComida()
        {
            var catalogo = RepositorioAlimentosFalso.ConCatalogoDePrueba();
            var plan = new AlimentacionRecomposicion(catalogo).GenerarPlan(Macros);

            var momentoDe = new Dictionary<string, string>
            {
                ["Desayuno"] = "desayuno",
                ["Almuerzo"] = "principal",
                ["Merienda"] = "snack",
                ["Cena"] = "principal"
            };

            foreach (var comida in plan.Comidas)
            {
                var momento = momentoDe[comida.NombreComida];
                foreach (var sustituto in comida.Porciones.SelectMany(p => p.Sustitutos))
                    Assert.True(sustituto.Alimento.VaEn(momento),
                        $"'{sustituto.Alimento.Nombre}' no va en '{comida.NombreComida}'.");
            }
        }

        [Fact]
        public void LaDescripcionDeLaPorcionIncluyeLosGramosYElAlimento()
        {
            var porcion = PlanDePrueba().Comidas.First().Porciones.First();

            Assert.Contains(" g de ", porcion.Descripcion);
            Assert.Contains(porcion.Alimento.Nombre, porcion.Descripcion);
        }
    }
}
