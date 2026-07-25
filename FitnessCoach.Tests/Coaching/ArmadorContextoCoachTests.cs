using FitnessCoach.Application.Coaching;
using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Coaching
{
    public class ArmadorContextoCoachTests
    {
        private static UsuarioPerfil Usuario()
        {
            var u = new UsuarioPerfil
            {
                Id = 1, IdentityUserId = "u1", Nombre = "Ana",
                Edad = 30, EstaturaCm = 165, PesoKg = 65,
                ObjetivoActual = new ObjetivoPerderPeso()
            };
            u.HistorialProgreso.Add(new RegistroProgreso { Id = 1, Fecha = DateTime.UtcNow, PesoKg = 65 });
            u.Preferencias.DietasSeguidas.Add("vegetariano");
            return u;
        }

        private static ArmadorContextoCoach Armador() => new(
            new GeneradorAlimentacionFalso(),
            new GeneradorRutinasFalso(),
            new RecordsFalso(),
            new DiarioFalso(),
            RepositorioAlimentosFalso.ConCatalogoDePrueba());

        [Fact]
        public void ElContextoIncluyeElPerfilYSusPreferencias()
        {
            var ctx = Armador().Construir(Usuario());

            Assert.Contains("Ana", ctx);
            Assert.Contains("Grasa", ctx);   // del nombre del objetivo "Pérdida de Grasa"
            Assert.Contains("vegetariano", ctx);
        }

        [Fact]
        public void ElContextoIncluyeElPlanLaRutinaYElDiario()
        {
            var ctx = Armador().Construir(Usuario());

            Assert.Contains("PLAN DE ALIMENTACION", ctx);
            Assert.Contains("Pechuga de pollo", ctx);      // del plan falso
            Assert.Contains("RUTINA", ctx);
            Assert.Contains("Sentadilla", ctx);            // de la rutina falsa
            Assert.Contains("DIARIO DE HOY", ctx);
        }

        [Fact]
        public void ElContextoIncluyeLosRecords()
        {
            var ctx = Armador().Construir(Usuario());

            Assert.Contains("RECORDS", ctx);
            Assert.Contains("Press de banca", ctx);
        }

        [Fact]
        public void ElContextoListaElCatalogoParaAnclarLasRecomendaciones()
        {
            var ctx = Armador().Construir(Usuario());

            // La lista de alimentos reales es lo que impide que la IA invente.
            Assert.Contains("ALIMENTOS DISPONIBLES", ctx);
            Assert.Contains("Tofu", ctx);
            Assert.Contains("Brócoli", ctx);
        }

        [Fact]
        public void ElContextoResumeLaSemanaConEntrenamientosYRacha()
        {
            var u = Usuario();
            u.EntrenamientosCompletados.Add(new EntrenamientoCompletado
            {
                Fecha = DateTime.UtcNow, NombreRutina = "Full Body", DuracionMinutos = 45
            });

            var ctx = Armador().Construir(u);

            Assert.Contains("ESTA SEMANA", ctx);
            Assert.Contains("Full Body", ctx);
            Assert.Contains("Racha actual: 1", ctx);
        }

        [Fact]
        public void UnEntrenamientoViejo_NoCuentaEnLaSemana()
        {
            var u = Usuario();
            u.EntrenamientosCompletados.Add(new EntrenamientoCompletado
            {
                Fecha = DateTime.UtcNow.AddDays(-30), NombreRutina = "Full Body", DuracionMinutos = 45
            });

            var ctx = Armador().Construir(u);

            Assert.DoesNotContain("Full Body", ctx);   // fuera de los 7 días
        }

        [Fact]
        public void UnBloqueQueFalla_NoTumbaTodoElContexto()
        {
            // El generador de plan lanza; el resto del contexto igual se arma.
            var armador = new ArmadorContextoCoach(
                new GeneradorAlimentacionQueFalla(),
                new GeneradorRutinasFalso(),
                new RecordsFalso(),
                new DiarioFalso(),
                RepositorioAlimentosFalso.ConCatalogoDePrueba());

            var ctx = armador.Construir(Usuario());

            Assert.DoesNotContain("PLAN DE ALIMENTACION", ctx);   // el bloque que falló se omite
            Assert.Contains("Ana", ctx);                          // el resto sigue
            Assert.Contains("RUTINA", ctx);
        }

        [Fact]
        public void SinUsuario_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => Armador().Construir(null!));
        }

        // ---- Fakes inline: devuelven datos conocidos para verificar el formato ----

        private sealed class GeneradorAlimentacionFalso : IGeneradorAlimentacion
        {
            public PlanAlimentacion GenerarPlanPara(UsuarioPerfil usuario)
            {
                var pollo = new Alimento
                {
                    Slug = "pechuga-de-pollo", Nombre = "Pechuga de pollo",
                    Categoria = "proteina", ProteinaPor100g = 22.5
                };
                var comida = new ComidaDia { NombreComida = "Almuerzo", Hora = "13:00" };
                comida.Porciones.Add(new PorcionAlimento { Alimento = pollo, Gramos = 150 });

                return new PlanAlimentacion
                {
                    NombrePlan = "Plan", Objetivo = "Perder grasa",
                    Objetivos = new ObjetivoMacros(1600, 120, 45, 150),
                    Comidas = { comida }
                };
            }
        }

        private sealed class GeneradorAlimentacionQueFalla : IGeneradorAlimentacion
        {
            public PlanAlimentacion GenerarPlanPara(UsuarioPerfil usuario) =>
                throw new InvalidOperationException("perfil sin datos válidos");
        }

        private sealed class GeneradorRutinasFalso : IGeneradorRutinas
        {
            public Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo, int semillaRotacion = 0)
            {
                var dia = new DiaEntrenamiento { NombreDia = "Dia 1", Enfoque = "Piernas" };
                dia.Ejercicios.Add(new EjercicioPrescrito
                {
                    Ejercicio = new Ejercicio { Slug = "sentadilla", Nombre = "Sentadilla" },
                    Series = 4, Repeticiones = "8-10"
                });
                return new Rutina { NombreRutina = "Rutina", Nivel = "Intermedio", Dias = { dia } };
            }
        }

        private sealed class RecordsFalso : IServicioRecords
        {
            public IReadOnlyList<RecordPersonal> ObtenerTodos(string identityUserId) => new[]
            {
                new RecordPersonal { EjercicioSlug = "press-banca", EjercicioNombre = "Press de banca", PesoKg = 80, Repeticiones = 5 }
            };
            public RecordPersonal? ObtenerDeEjercicio(string identityUserId, string ejercicioSlug) => null;
            public ResultadoRecord Registrar(string identityUserId, string ejercicioSlug, string ejercicioNombre, double pesoKg, int repeticiones) =>
                throw new NotImplementedException();
            public bool Eliminar(string identityUserId, int recordId) => false;
        }

        private sealed class DiarioFalso : IServicioDiario
        {
            public void Registrar(UsuarioPerfil usuario, string alimentoSlug, double gramos, DateOnly dia) { }
            public void Borrar(UsuarioPerfil usuario, int registroId) { }
            public ResumenDiario ResumenDelDia(UsuarioPerfil usuario, DateOnly dia) =>
                new(dia, new ObjetivoMacros(1600, 120, 45, 150), Array.Empty<RegistroComida>());
        }
    }
}
