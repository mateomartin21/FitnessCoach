using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Entrenamiento
{
    /// <summary>
    /// Cambiar un ejercicio de la rutina por otro que trabaje lo mismo (D-36).
    /// </summary>
    public class SustitucionEjerciciosTests
    {
        private static RepositorioEjerciciosFalso Catalogo() => new(
            new Ejercicio { Slug = "press-banca", Nombre = "Press de banca", GrupoMuscular = "pectorals", Equipo = "barbell" },
            new Ejercicio { Slug = "flexiones", Nombre = "Flexiones", GrupoMuscular = "pectorals", Equipo = "bodyweight" },
            new Ejercicio { Slug = "aperturas", Nombre = "Aperturas con mancuerna", GrupoMuscular = "pectorals", Equipo = "dumbbell" },
            new Ejercicio { Slug = "sentadilla", Nombre = "Sentadilla", GrupoMuscular = "quads", Equipo = "barbell" });

        private static UsuarioPerfil Usuario() => new() { Id = 1, ObjetivoActual = new ObjetivoGanarMusculo() };

        private static ServicioSustitucionEjercicios Servicio() => new(Catalogo());

        [Fact]
        public void Alternativas_SoloOfreceElMismoGrupoMuscularYSinElQueYaEstaEnUso()
        {
            var slugs = Servicio().Alternativas(Usuario(), "press-banca").Select(e => e.Slug).ToList();

            Assert.Equal(new[] { "aperturas", "flexiones" }, slugs.Order().ToArray());
            Assert.DoesNotContain("sentadilla", slugs);
            Assert.DoesNotContain("press-banca", slugs);
        }

        [Fact]
        public void Alternativas_RespetanElEquipoDelUsuario()
        {
            var usuario = Usuario();
            usuario.PreferenciasEntrenamiento.EquipoDisponible.Add("peso-corporal");

            var slugs = Servicio().Alternativas(usuario, "press-banca").Select(e => e.Slug).ToList();

            Assert.Equal(new[] { "flexiones" }, slugs.ToArray());
        }

        [Fact]
        public void Sustituir_RechazaUnEjercicioDeOtroGrupoMuscular()
        {
            var usuario = Usuario();

            Assert.False(Servicio().Sustituir(usuario, "press-banca", "sentadilla"));
            Assert.Empty(usuario.PreferenciasEntrenamiento.Sustituciones);
        }

        [Fact]
        public void Sustituir_RechazaUnSlugQueNoExiste()
        {
            var usuario = Usuario();

            Assert.False(Servicio().Sustituir(usuario, "press-banca", "no-existe"));
            Assert.Empty(usuario.PreferenciasEntrenamiento.Sustituciones);
        }

        [Fact]
        public void CambiarDosVeces_GuardaSiempreElOriginalComoClave()
        {
            var usuario = Usuario();
            var servicio = Servicio();

            servicio.Sustituir(usuario, "press-banca", "flexiones");
            servicio.Sustituir(usuario, "press-banca", "aperturas");

            // Una sola entrada: si encadenara flexiones→aperturas, el cambio se perdería
            // en cuanto la estrategia dejara de elegir flexiones.
            var sustituciones = usuario.PreferenciasEntrenamiento.Sustituciones;
            var unica = Assert.Single(sustituciones);
            Assert.Equal("press-banca", unica.Key);
            Assert.Equal("aperturas", unica.Value);
        }

        [Fact]
        public void ElegirDeNuevoElOriginal_EsDeshacer()
        {
            var usuario = Usuario();
            var servicio = Servicio();

            servicio.Sustituir(usuario, "press-banca", "flexiones");
            servicio.Sustituir(usuario, "press-banca", "press-banca");

            Assert.Empty(usuario.PreferenciasEntrenamiento.Sustituciones);
        }

        [Fact]
        public void LaRutinaMuestraElReemplazoYRecuerdaCualEraElOriginal()
        {
            var usuario = Usuario();
            usuario.PreferenciasEntrenamiento.Sustituciones["pectorals-ejercicio-1"] = "flexiones";

            var catalogo = RepositorioEjerciciosFalso.ConGrupos(3,
                "pectorals", "triceps", "lats", "upper-back", "biceps", "quads",
                "hamstrings", "calves", "delts", "traps", "glutes", "abs", "cardio");
            // El reemplazo tiene que existir en el mismo catálogo que compone la rutina.
            var conFlexiones = new RepositorioEjerciciosFalso(
                catalogo.ObtenerTodos()
                    .Append(new Ejercicio { Slug = "flexiones", Nombre = "Flexiones", GrupoMuscular = "pectorals", Equipo = "bodyweight" })
                    .ToArray());

            var rutina = new GeneradorRutinasService(conFlexiones)
                .GenerarRutinaParaObjetivo(usuario.ObjetivoActual!, usuario.Id, usuario.PreferenciasEntrenamiento);

            var cambiado = rutina.Dias.SelectMany(d => d.Ejercicios)
                .FirstOrDefault(e => e.Ejercicio.Slug == "flexiones");

            Assert.NotNull(cambiado);
            Assert.True(cambiado!.EsSustituido);
            Assert.Equal("pectorals-ejercicio-1", cambiado.SlugOriginal);
            Assert.Equal("pectorals-ejercicio-1", cambiado.SlugDeReferencia);
        }

        [Fact]
        public void UnReemplazoQueYaNoEstaEnElCatalogo_DejaElOriginal()
        {
            var usuario = Usuario();
            usuario.PreferenciasEntrenamiento.Sustituciones["press-banca"] = "borrado-del-catalogo";

            var rutina = new GeneradorRutinasService(Catalogo())
                .GenerarRutinaParaObjetivo(usuario.ObjetivoActual!, usuario.Id, usuario.PreferenciasEntrenamiento);

            Assert.All(rutina.Dias.SelectMany(d => d.Ejercicios),
                e => Assert.NotEqual("borrado-del-catalogo", e.Ejercicio.Slug));
        }
    }
}
