using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Gamificacion;
using FitnessCoach.Domain.Models.Objetivos;
using Xunit;

namespace FitnessCoach.Tests.Gamificacion
{
    public class ServicioGamificacionTests
    {
        [Fact]
        public void ElXpTotalSumaHechos_LogrosYMisiones()
        {
            // 1 entreno esta semana: base 50; logros primer-entreno (25) + con-objetivo (20) = 45;
            // misiones: ninguna cumplida (hace falta 3 entrenos). Total 95.
            var stats = EstadisticasUsuario.Vacias with
            {
                TotalEntrenamientos = 1,
                EntrenamientosEstaSemana = 1,
                TieneObjetivo = true
            };

            var resumen = ServicioGamificacion.Armar(stats);

            Assert.Equal(95, resumen.Nivel.XpTotal);
            Assert.Equal(2, resumen.LogrosDesbloqueados);
        }

        [Fact]
        public void LasMisionesCumplidasTambienDanXp()
        {
            var sinMision = EstadisticasUsuario.Vacias with { EntrenamientosEstaSemana = 2 };
            var conMision = sinMision with { EntrenamientosEstaSemana = 3 };  // cumple "Constancia semanal" (50)

            var diferencia = ServicioGamificacion.Armar(conMision).Nivel.XpTotal
                             - ServicioGamificacion.Armar(sinMision).Nivel.XpTotal;

            // +50 de la misión, y de paso el logro "Semana de fuego" (50): 100 en total.
            Assert.Equal(100, diferencia);
        }

        [Fact]
        public void ElSnapshotCuentaLosHechosDelPerfil()
        {
            var u = new UsuarioPerfil
            {
                IdentityUserId = "u1", Nombre = "Ana",
                ObjetivoActual = new ObjetivoPerderPeso()
            };
            // Ambos son de hoy: caen sí o sí en la semana de calendario actual, sin
            // depender del día en que corran las pruebas.
            u.EntrenamientosCompletados.Add(new EntrenamientoCompletado { Fecha = DateTime.UtcNow, NombreRutina = "Full Body", DuracionMinutos = 40 });
            u.EntrenamientosCompletados.Add(new EntrenamientoCompletado { Fecha = DateTime.UtcNow, NombreRutina = "Piernas", DuracionMinutos = 50 });
            u.HistorialProgreso.Add(new RegistroProgreso { Fecha = DateTime.UtcNow, PesoKg = 65 });
            u.RecordsPersonales.Add(new RecordPersonal { EjercicioSlug = "sentadilla", EjercicioNombre = "Sentadilla", PesoKg = 60, Repeticiones = 5 });

            var stats = ServicioGamificacion.Snapshot(u);

            Assert.Equal(2, stats.TotalEntrenamientos);
            Assert.Equal(2, stats.EntrenamientosEstaSemana);
            Assert.Equal(1, stats.TotalRecords);
            Assert.Equal(1, stats.TotalRegistrosPeso);
            Assert.True(stats.TieneObjetivo);
            Assert.True(stats.RachaActual >= 1);
        }

        [Fact]
        public void ElDiarioCuentaDiasDistintos_NoRegistros()
        {
            var u = new UsuarioPerfil { IdentityUserId = "u1" };
            var hoy = DateTime.UtcNow;
            // Tres registros pero de dos días distintos.
            u.Diario.Add(new RegistroComida { Fecha = hoy, AlimentoNombre = "A" });
            u.Diario.Add(new RegistroComida { Fecha = hoy, AlimentoNombre = "B" });
            u.Diario.Add(new RegistroComida { Fecha = hoy.AddDays(-2), AlimentoNombre = "C" });

            var stats = ServicioGamificacion.Snapshot(u);

            Assert.Equal(2, stats.DiasConDiario);
        }

        [Fact]
        public void UnEntrenamientoViejo_NoCuentaEnLaSemana()
        {
            var u = new UsuarioPerfil { IdentityUserId = "u1" };
            u.EntrenamientosCompletados.Add(new EntrenamientoCompletado { Fecha = DateTime.UtcNow.AddDays(-30), NombreRutina = "Viejo", DuracionMinutos = 30 });

            var stats = ServicioGamificacion.Snapshot(u);

            Assert.Equal(1, stats.TotalEntrenamientos);
            Assert.Equal(0, stats.EntrenamientosEstaSemana);
        }
    }
}
