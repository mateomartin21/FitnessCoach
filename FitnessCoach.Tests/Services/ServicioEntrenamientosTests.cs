using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    public class ServicioEntrenamientosTests
    {
        private const string IdentityAna = "identity-ana";
        private const string IdentityBruno = "identity-bruno";

        private static (ServicioEntrenamientos servicio, UsuarioPerfil ana) Montar(
            params EntrenamientoCompletado[] entrenamientos)
        {
            var ana = new UsuarioPerfil
            {
                IdentityUserId = IdentityAna,
                Nombre = "Ana",
                Edad = 30,
                EstaturaCm = 165,
                PesoKg = 62,
                // Sin objetivo no hay rutina y el servicio no acepta ningún entrenamiento.
                ObjetivoActual = new ObjetivoPerderPeso()
            };

            int siguienteId = 1;
            foreach (var entrenamiento in entrenamientos)
            {
                if (entrenamiento.Id == 0) entrenamiento.Id = siguienteId++;
                ana.EntrenamientosCompletados.Add(entrenamiento);
            }

            var repositorio = new RepositorioUsuarioFalso(ana);
            var perfiles = new ServicioPerfilUsuario(repositorio);
            return (new ServicioEntrenamientos(perfiles, new GeneradorRutinasFalso()), ana);
        }

        private static EntrenamientoCompletado Entrenamiento(int diasAtras, string nombre = GeneradorRutinasFalso.Dia1) => new()
        {
            Fecha = DateTime.UtcNow.AddDays(-diasAtras),
            NombreRutina = nombre,
            DuracionMinutos = 45
        };

        [Fact]
        public void Registrar_GuardaElEntrenamiento()
        {
            var (servicio, ana) = Montar();

            var entrenamiento = servicio.Registrar(IdentityAna, GeneradorRutinasFalso.Dia1, 50, "Pesado");

            Assert.Equal(GeneradorRutinasFalso.Dia1, entrenamiento.NombreRutina);
            Assert.Equal(50, entrenamiento.DuracionMinutos);
            Assert.Single(ana.EntrenamientosCompletados);
        }

        [Fact]
        public void Registrar_PoneLaFechaEnUtc()
        {
            var (servicio, _) = Montar();

            var entrenamiento = servicio.Registrar(IdentityAna, GeneradorRutinasFalso.Dia2, 40, null);

            Assert.Equal(DateTimeKind.Utc, entrenamiento.Fecha.Kind);
        }

        [Fact]
        public void OpcionesDeRutina_SonLosDiasDeLaRutinaReal()
        {
            var (servicio, _) = Montar();

            Assert.Equal(new[] { GeneradorRutinasFalso.Dia1, GeneradorRutinasFalso.Dia2 },
                         servicio.OpcionesDeRutina(IdentityAna));
        }

        [Fact]
        public void Registrar_UnEntrenamientoInventado_NoSeGuarda()
        {
            // Si la regla viviera solo en la pantalla, la API la saltearía (D-26).
            var (servicio, ana) = Montar();

            Assert.Throws<ArgumentException>(() =>
                servicio.Registrar(IdentityAna, "Maratón inventado", 300, null));

            Assert.Empty(ana.EntrenamientosCompletados);
        }

        [Fact]
        public void SinObjetivo_NoHayOpcionesNiSePuedeRegistrar()
        {
            var (servicio, ana) = Montar();
            ana.ObjetivoActual = null;

            Assert.Empty(servicio.OpcionesDeRutina(IdentityAna));
            Assert.Throws<ArgumentException>(() =>
                servicio.Registrar(IdentityAna, GeneradorRutinasFalso.Dia1, 45, null));
        }

        [Fact]
        public void ObtenerHistorial_DevuelveDelMasRecienteAlMasAntiguo()
        {
            var (servicio, _) = Montar(
                Entrenamiento(diasAtras: 5, nombre: "Viejo"),
                Entrenamiento(diasAtras: 0, nombre: "Hoy"),
                Entrenamiento(diasAtras: 2, nombre: "Medio"));

            var historial = servicio.ObtenerHistorial(IdentityAna);

            Assert.Equal(new[] { "Hoy", "Medio", "Viejo" }, historial.Select(e => e.NombreRutina));
        }

        [Fact]
        public void ObtenerRachas_ConDiasConsecutivos_CuentaLaRacha()
        {
            var (servicio, _) = Montar(
                Entrenamiento(diasAtras: 2),
                Entrenamiento(diasAtras: 1),
                Entrenamiento(diasAtras: 0));

            var rachas = servicio.ObtenerRachas(IdentityAna);

            Assert.Equal(3, rachas.Actual);
            Assert.Equal(3, rachas.MasLarga);
        }

        [Fact]
        public void ObtenerRachas_CuentaLosDiasEnLaZonaDelUsuario()
        {
            // Dos entrenamientos del mismo día UTC (02:00 y 20:00): en UTC son un día de
            // racha, en México (UTC-6) el primero cae la noche anterior y son dos (D-25).
            var rachaUtc = RachaConZona("UTC");
            var rachaMexico = RachaConZona("America/Mexico_City");

            Assert.Equal(1, rachaUtc.Actual);
            Assert.Equal(1, rachaUtc.MasLarga);
            Assert.Equal(2, rachaMexico.Actual);
            Assert.Equal(2, rachaMexico.MasLarga);
        }

        /// <summary>Los mismos instantes, leídos desde la zona indicada.</summary>
        private static Rachas RachaConZona(string zona)
        {
            var ayerUtc = DateTime.UtcNow.Date.AddDays(-1);

            var (servicio, ana) = Montar(
                new EntrenamientoCompletado { Fecha = ayerUtc.AddHours(2), NombreRutina = "A", DuracionMinutos = 40 },
                new EntrenamientoCompletado { Fecha = ayerUtc.AddHours(20), NombreRutina = "B", DuracionMinutos = 40 });
            ana.ZonaHoraria = zona;

            return servicio.ObtenerRachas(IdentityAna);
        }

        [Fact]
        public void ObtenerRachas_SinEntrenamientos_DevuelveVacia()
        {
            var (servicio, _) = Montar();

            Assert.Equal(Rachas.Vacia, servicio.ObtenerRachas(IdentityAna));
        }

        [Fact]
        public void Eliminar_QuitaElEntrenamiento()
        {
            var (servicio, ana) = Montar(Entrenamiento(diasAtras: 1));
            var id = ana.EntrenamientosCompletados[0].Id;

            Assert.True(servicio.Eliminar(IdentityAna, id));
            Assert.Empty(ana.EntrenamientosCompletados);
        }

        [Fact]
        public void Eliminar_UnEntrenamientoDeOtraCuenta_DevuelveFalse()
        {
            var (servicio, ana) = Montar(Entrenamiento(diasAtras: 1));
            var idDeAna = ana.EntrenamientosCompletados[0].Id;

            Assert.False(servicio.Eliminar(IdentityBruno, idDeAna));
            Assert.Single(ana.EntrenamientosCompletados);
        }

        [Fact]
        public void ObtenerHistorial_DeOtraCuenta_NoVeLosAjenos()
        {
            var (servicio, _) = Montar(Entrenamiento(diasAtras: 1));

            Assert.Empty(servicio.ObtenerHistorial(IdentityBruno));
        }
    }
}
