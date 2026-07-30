using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    /// <summary>
    /// La zona horaria es la que decide "qué día es" para el usuario: de acá cuelgan las
    /// rachas, las misiones y el día del diario (D-25). Si resolviera mal, todo eso se
    /// corre un día sin que nada falle a la vista.
    /// </summary>
    public class ZonaHorariaUsuarioTests
    {
        [Fact]
        public void TodasLasZonasDelSelectorExistenEnElSistema()
        {
            // Un id mal escrito en el catálogo se vería en la pantalla como una opción
            // normal, pero al guardarla caería silenciosamente a la zona por defecto.
            foreach (var (id, etiqueta) in ZonaHorariaUsuario.Comunes)
                Assert.True(ZonaHorariaUsuario.EsValida(id), $"Zona inválida en el catálogo: {id} ({etiqueta})");
        }

        [Fact]
        public void LaZonaPorDefectoEsValida()
        {
            Assert.True(ZonaHorariaUsuario.EsValida(ZonaHorariaUsuario.PorDefecto));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("America/Ciudad_Inventada")]
        public void UnaZonaAusenteOInventadaCaeALaPorDefecto(string? id)
        {
            var zona = ZonaHorariaUsuario.Resolver(id);

            Assert.Equal(ZonaHorariaUsuario.Resolver(ZonaHorariaUsuario.PorDefecto).Id, zona.Id);
        }

        [Fact]
        public void EsValidaRechazaLoQueNoResuelve()
        {
            Assert.False(ZonaHorariaUsuario.EsValida("cualquier cosa"));
            Assert.False(ZonaHorariaUsuario.EsValida(""));
        }

        [Fact]
        public void LaZonaSaleDelPerfil()
        {
            var u = new UsuarioPerfil { ZonaHoraria = "America/Argentina/Buenos_Aires" };

            var zona = ZonaHorariaUsuario.De(u);

            // Buenos Aires está en UTC-3 todo el año; Ciudad de México nunca.
            Assert.Equal(TimeSpan.FromHours(-3), zona.GetUtcOffset(new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void ALocalTrataLaFechaGuardadaComoUtc()
        {
            // Así vuelven de la base: sin Kind. Si se interpretaran como hora local del
            // servidor, la conversión sumaría (o restaría) dos veces el desfase.
            var guardada = new DateTime(2026, 7, 29, 3, 0, 0, DateTimeKind.Unspecified);
            var zonaMexico = ZonaHorariaUsuario.Resolver("America/Mexico_City");

            var local = ZonaHorariaUsuario.ALocal(guardada, zonaMexico);

            // 03:00 UTC del 29 es todavía el 28 por la noche en Ciudad de México (UTC-6).
            Assert.Equal(new DateTime(2026, 7, 28, 21, 0, 0), local);
        }

        [Fact]
        public void ElDiaDependeDeLaZona()
        {
            var guardada = new DateTime(2026, 7, 29, 3, 0, 0, DateTimeKind.Unspecified);

            var enMexico = DateOnly.FromDateTime(ZonaHorariaUsuario.ALocal(guardada, ZonaHorariaUsuario.Resolver("America/Mexico_City")));
            var enMadrid = DateOnly.FromDateTime(ZonaHorariaUsuario.ALocal(guardada, ZonaHorariaUsuario.Resolver("Europe/Madrid")));

            // El mismo instante: 28 en México, 29 en Madrid. Esto es exactamente lo que
            // hacía que la racha se cortara antes de tiempo para el usuario.
            Assert.Equal(new DateOnly(2026, 7, 28), enMexico);
            Assert.Equal(new DateOnly(2026, 7, 29), enMadrid);
        }
    }
}
