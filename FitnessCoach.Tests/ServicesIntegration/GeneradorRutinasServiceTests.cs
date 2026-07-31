using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.ServicesIntegration
{
    public class GeneradorRutinasServiceTests
    {
        // Todos los grupos que las tres estrategias piden, con varios ejercicios cada uno
        // para que la selección tenga entre qué elegir.
        private static readonly string[] TodosLosGrupos =
        {
            "pectorals", "triceps", "lats", "upper-back", "biceps",
            "quads", "hamstrings", "calves", "delts", "traps",
            "glutes", "abs", "cardio"
        };

        private static GeneradorRutinasService Generador(int ejerciciosPorGrupo = 5) =>
            new(RepositorioEjerciciosFalso.ConGrupos(ejerciciosPorGrupo, TodosLosGrupos));

        [Fact]
        public void GenerarRutinaParaObjetivo_ConPerderPeso_SeleccionaEstrategiaCorrecta()
        {
            var rutina = Generador().GenerarRutinaParaObjetivo(new ObjetivoPerderPeso());

            Assert.Equal("Principiante/Intermedio", rutina.Nivel);
        }

        [Fact]
        public void GenerarRutinaParaObjetivo_ConGanarMusculo_SeleccionaEstrategiaCorrecta()
        {
            var rutina = Generador().GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo());

            Assert.Equal("Avanzado", rutina.Nivel);
        }

        [Fact]
        public void GenerarRutinaParaObjetivo_ConRecomposicion_SeleccionaEstrategiaCorrecta()
        {
            var rutina = Generador().GenerarRutinaParaObjetivo(new ObjetivoRecomposicion());

            Assert.Equal("Intermedio", rutina.Nivel);
        }

        [Fact]
        public void GenerarRutinaParaObjetivo_TodosLosDiasTienenCalentamientoYEnfriamiento()
        {
            var rutina = Generador().GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo());

            // El Decorator debe aplicarse a CADA día, no solo al primero
            Assert.All(rutina.Dias, dia =>
            {
                Assert.Equal("Calentamiento General", dia.Ejercicios[0].Nombre);
                Assert.Equal("Enfriamiento y Estiramientos", dia.Ejercicios[^1].Nombre);
            });
        }

        // --- Composición desde el catálogo ---

        [Fact]
        public void LosEjerciciosSalenDelCatalogo_NoEstanIncrustadosEnLaEstrategia()
        {
            var rutina = Generador().GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo());

            // Todo lo que no sea calentamiento/enfriamiento tiene que venir del catálogo,
            // y el catálogo falso nombra sus ejercicios "<grupo> <n>".
            var delCatalogo = rutina.Dias
                .SelectMany(d => d.Ejercicios)
                .Where(e => e.Ejercicio.GrupoMuscular != string.Empty)
                .ToList();

            Assert.NotEmpty(delCatalogo);
            Assert.All(delCatalogo, e => Assert.StartsWith("https://cdn.example/", e.Ejercicio.UrlGif));
        }

        [Fact]
        public void ConCatalogoVacio_NoSeCaeNiDevuelveDiasHuecos()
        {
            var generador = new GeneradorRutinasService(new RepositorioEjerciciosFalso());

            var rutina = generador.GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo());

            // Sin ejercicios que prescribir no hay días: mejor una rutina vacía que
            // una llena de días con solo calentamiento.
            Assert.Empty(rutina.Dias);
        }

        [Fact]
        public void LaMismaSemilla_DaSiempreLaMismaRutina()
        {
            var primera = Generador().GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo(), semillaRotacion: 7);
            var segunda = Generador().GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo(), semillaRotacion: 7);

            Assert.Equal(Slugs(primera), Slugs(segunda));
        }

        [Fact]
        public void SemillasDistintas_DanEjerciciosDistintos()
        {
            // Es el objetivo de la fase: dos personas con el mismo objetivo no ven lo mismo.
            var deUno = Generador(ejerciciosPorGrupo: 10)
                .GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo(), semillaRotacion: 1);
            var deOtro = Generador(ejerciciosPorGrupo: 10)
                .GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo(), semillaRotacion: 2);

            Assert.NotEqual(Slugs(deUno), Slugs(deOtro));
        }

        [Fact]
        public void NoRepiteElMismoEjercicioDentroDeUnDia()
        {
            var rutina = Generador(ejerciciosPorGrupo: 3).GenerarRutinaParaObjetivo(new ObjetivoRecomposicion());

            Assert.All(rutina.Dias, dia =>
            {
                var slugs = dia.Ejercicios.Select(e => e.Ejercicio.Slug).ToList();
                Assert.Equal(slugs.Count, slugs.Distinct().Count());
            });
        }

        [Fact]
        public void LaPrescripcionLaPoneLaEstrategia_NoElCatalogo()
        {
            var rutina = Generador().GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo());

            var primerDia = rutina.Dias[0];
            // Sin contar calentamiento (1 serie) ni enfriamiento
            var pectoral = primerDia.Ejercicios.First(e => e.Ejercicio.GrupoMuscular == "pectorals");

            Assert.Equal(4, pectoral.Series);
            Assert.Equal("8-10", pectoral.Repeticiones);
        }

        // Nombre completo: "Domain" a secas colisiona con el namespace FitnessCoach.Tests.Domain
        private static List<string> Slugs(FitnessCoach.Domain.Models.Entrenamiento.Rutina rutina) =>
            rutina.Dias.SelectMany(d => d.Ejercicios).Select(e => e.Ejercicio.Slug).ToList();
    }
}
