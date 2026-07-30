using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
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
                PesoKg = 62
            };

            int siguienteId = 1;
            foreach (var entrenamiento in entrenamientos)
            {
                if (entrenamiento.Id == 0) entrenamiento.Id = siguienteId++;
                ana.EntrenamientosCompletados.Add(entrenamiento);
            }

            var repositorio = new RepositorioUsuarioFalso(ana);
            var perfiles = new ServicioPerfilUsuario(repositorio);
            return (new ServicioEntrenamientos(perfiles), ana);
        }

        private static EntrenamientoCompletado Entrenamiento(int diasAtras, string nombre = "Full Body") => new()
        {
            Fecha = DateTime.UtcNow.AddDays(-diasAtras),
            NombreRutina = nombre,
            DuracionMinutos = 45
        };

        [Fact]
        public void Registrar_GuardaElEntrenamiento()
        {
            var (servicio, ana) = Montar();

            var entrenamiento = servicio.Registrar(IdentityAna, "Piernas", 50, "Pesado");

            Assert.Equal("Piernas", entrenamiento.NombreRutina);
            Assert.Equal(50, entrenamiento.DuracionMinutos);
            Assert.Single(ana.EntrenamientosCompletados);
        }

        [Fact]
        public void Registrar_PoneLaFechaEnUtc()
        {
            var (servicio, _) = Montar();

            var entrenamiento = servicio.Registrar(IdentityAna, "Torso", 40, null);

            Assert.Equal(DateTimeKind.Utc, entrenamiento.Fecha.Kind);
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
            // Dos entrenamientos del MISMO día UTC (ayer 02:00 y ayer 20:00). En UTC son un
            // solo día de racha; en Ciudad de México (UTC-6) el de las 02:00 cae la noche
            // anterior, así que son dos días consecutivos. El mismo hecho, distinta racha:
            // eso es exactamente lo que D-25 rompía al contar con la hora del servidor.
            var rachaUtc = RachaConZona("UTC");
            var rachaMexico = RachaConZona("America/Mexico_City");

            Assert.Equal(1, rachaUtc.Actual);
            Assert.Equal(1, rachaUtc.MasLarga);
            Assert.Equal(2, rachaMexico.Actual);
            Assert.Equal(2, rachaMexico.MasLarga);
        }

        /// <summary>Los mismos dos instantes, leídos desde la zona indicada.</summary>
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
