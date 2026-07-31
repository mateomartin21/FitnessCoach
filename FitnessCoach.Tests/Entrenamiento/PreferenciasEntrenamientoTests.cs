using System.Text.Json;
using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Catalogos;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Entrenamiento
{
    /// <summary>
    /// El equipo del usuario filtra el catálogo antes de que la estrategia arme la rutina.
    /// Sin esto, los 1323 ejercicios daban siempre los mismos 16-23 por perfil.
    /// </summary>
    public class PreferenciasEntrenamientoTests
    {
        private static readonly string[] Grupos =
        {
            "pectorals", "triceps", "lats", "upper-back", "biceps",
            "quads", "hamstrings", "calves", "delts", "traps",
            "glutes", "abs", "cardio"
        };

        /// <summary>Un ejercicio de cada equipo en cada grupo, para que el filtro tenga de dónde elegir.</summary>
        private static RepositorioEjerciciosFalso CatalogoVariado()
        {
            string[] equipos = { "bodyweight", "dumbbell", "band", "barbell", "cable", "lever" };
            var ejercicios = new List<Ejercicio>();

            foreach (var grupo in Grupos)
                foreach (var equipo in equipos)
                    ejercicios.Add(new Ejercicio
                    {
                        Slug = $"{grupo}-{equipo}",
                        Nombre = $"{grupo} con {equipo}",
                        GrupoMuscular = grupo,
                        Equipo = equipo
                    });

            return new RepositorioEjerciciosFalso(ejercicios.ToArray());
        }

        private static IEnumerable<Ejercicio> EjerciciosDe(Rutina rutina) =>
            rutina.Dias.SelectMany(d => d.Ejercicios).Select(e => e.Ejercicio!);

        [Fact]
        public void SinEquipoMarcado_LaRutinaPuedeUsarCualquierEquipo()
        {
            var generador = new GeneradorRutinasService(CatalogoVariado());

            var rutina = generador.GenerarRutinaParaObjetivo(
                new ObjetivoGanarMusculo(), semillaRotacion: 3, preferencias: new PreferenciasEntrenamiento());

            Assert.NotEmpty(rutina.Dias);
        }

        [Fact]
        public void ConSoloPesoCorporal_NingunEjercicioPideMaterial()
        {
            var generador = new GeneradorRutinasService(CatalogoVariado());
            var soloEnCasa = new PreferenciasEntrenamiento { EquipoDisponible = { "peso-corporal" } };

            var rutina = generador.GenerarRutinaParaObjetivo(
                new ObjetivoGanarMusculo(), semillaRotacion: 3, preferencias: soloEnCasa);

            // Calentamiento y enfriamiento los agregan los decoradores y no salen del catálogo.
            var delCatalogo = EjerciciosDe(rutina).Where(e => Grupos.Contains(e.GrupoMuscular));

            Assert.NotEmpty(delCatalogo);
            Assert.All(delCatalogo, e => Assert.Equal("bodyweight", e.Equipo));
        }

        [Fact]
        public void MismoObjetivoYMismaSemilla_DistintoEquipoDaRutinasDistintas()
        {
            var generador = new GeneradorRutinasService(CatalogoVariado());

            var enCasa = generador.GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo(), 42,
                new PreferenciasEntrenamiento { EquipoDisponible = { "peso-corporal", "bandas" } });
            var enGimnasio = generador.GenerarRutinaParaObjetivo(new ObjetivoGanarMusculo(), 42,
                new PreferenciasEntrenamiento { EquipoDisponible = { "barra", "maquinas" } });

            var slugsCasa = EjerciciosDe(enCasa).Select(e => e.Slug).ToList();
            var slugsGimnasio = EjerciciosDe(enGimnasio).Select(e => e.Slug).ToList();

            Assert.NotEqual(slugsCasa, slugsGimnasio);
        }

        [Fact]
        public void SiElEquipoDejaUnGrupoSinNada_PrefiereOtroEjercicioAntesQuePerderElDia()
        {
            // Catálogo donde 'quads' solo existe con barra: alguien que entrena en casa
            // no puede quedarse sin pierna.
            var catalogo = new RepositorioEjerciciosFalso(
                new Ejercicio { Slug = "quads-barbell", Nombre = "Sentadilla", GrupoMuscular = "quads", Equipo = "barbell" },
                new Ejercicio { Slug = "abs-bodyweight", Nombre = "Plancha", GrupoMuscular = "abs", Equipo = "bodyweight" },
                new Ejercicio { Slug = "cardio-bodyweight", Nombre = "Trote", GrupoMuscular = "cardio", Equipo = "bodyweight" });

            var generador = new GeneradorRutinasService(catalogo);
            var enCasa = new PreferenciasEntrenamiento { EquipoDisponible = { "peso-corporal" } };

            var rutina = generador.GenerarRutinaParaObjetivo(new ObjetivoPerderPeso(), 1, enCasa);

            Assert.Contains(EjerciciosDe(rutina), e => e.Slug == "quads-barbell");
        }

        [Fact]
        public void Permite_DejaPasarLosEjerciciosSinGrupoDeEquipo()
        {
            var prefs = new PreferenciasEntrenamiento { EquipoDisponible = { "peso-corporal" } };

            Assert.True(prefs.Permite(new Ejercicio { Slug = "x", Equipo = "other" }));
            Assert.False(prefs.Permite(new Ejercicio { Slug = "y", Equipo = "barbell" }));
        }

        private static List<Ejercicio> CatalogoReal()
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "Data", "catalogo-ejercicios.json");
            var catalogo = JsonSerializer.Deserialize<List<Ejercicio>>(
                File.ReadAllText(ruta), new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(catalogo);
            return catalogo!;
        }

        [Fact]
        public void TodoGrupoYEquipoDelCatalogoRealTieneEtiquetaEnEspanol()
        {
            var catalogo = CatalogoReal();

            var gruposSinTraducir = catalogo.Select(e => e.GrupoMuscular)
                .Where(g => !string.IsNullOrWhiteSpace(g) && !EtiquetasEjercicio.ConoceGrupoMuscular(g))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var equiposSinTraducir = catalogo.Select(e => e.Equipo)
                .Where(q => !string.IsNullOrWhiteSpace(q) && !EtiquetasEjercicio.ConoceEquipo(q))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            Assert.True(gruposSinTraducir.Count == 0, $"Grupos sin etiqueta: {string.Join(", ", gruposSinTraducir)}");
            Assert.True(equiposSinTraducir.Count == 0, $"Equipos sin etiqueta: {string.Join(", ", equiposSinTraducir)}");
        }

        [Fact]
        public void TodoEquipoDelCatalogoRealCaeEnAlgunGrupo()
        {
            var catalogo = CatalogoReal();

            var cubiertos = EquipoEntrenamiento.EquiposDe(
                EquipoEntrenamiento.Disponibles.Select(g => g.Valor));

            // "other" se deja pasar siempre, así que no necesita grupo.
            var sinGrupo = catalogo
                .Select(e => e.Equipo ?? string.Empty)
                .Where(equipo => !cubiertos.Contains(equipo) &&
                                 !string.Equals(equipo, "other", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Assert.True(sinGrupo.Count == 0,
                $"Equipos del catálogo sin grupo en EquipoEntrenamiento: {string.Join(", ", sinGrupo)}");
        }
    }
}
