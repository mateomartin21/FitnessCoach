using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Gamificacion;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Arma el estado de gamificación del usuario a partir de sus hechos. No guarda nada
    /// propio: cada vez lee el perfil, lo traduce a un <see cref="EstadisticasUsuario"/> y
    /// deja que los calculadores de dominio (XP, nivel, logros, misiones) hagan el resto.
    /// Por eso no puede desincronizarse con la realidad ni necesita migración.
    ///
    /// El XP total suma tres fuentes: los hechos (base), los logros desbloqueados y las
    /// misiones cumplidas de la semana.
    /// </summary>
    public class ServicioGamificacion : IServicioGamificacion
    {
        private readonly IServicioPerfilUsuario _perfiles;

        public ServicioGamificacion(IServicioPerfilUsuario perfiles)
        {
            _perfiles = perfiles;
        }

        public ResumenGamificacion Obtener(string identityUserId) =>
            Armar(Estadisticas(identityUserId));

        public EstadisticasUsuario Estadisticas(string identityUserId) =>
            Snapshot(_perfiles.ObtenerOCrear(identityUserId));

        /// <summary>Junta nivel, logros y misiones desde un snapshot. Pública para poder probarla sin perfil.</summary>
        public static ResumenGamificacion Armar(EstadisticasUsuario stats)
        {
            var xpTotal = CalculadorXP.Base(stats)
                          + EvaluadorLogros.XpDesbloqueado(stats)
                          + CalculadorMisiones.XpCumplido(stats);

            return new ResumenGamificacion(
                CalculadorNivel.Calcular(xpTotal),
                EvaluadorLogros.Evaluar(stats),
                CalculadorMisiones.Evaluar(stats));
        }

        /// <summary>
        /// Traduce el perfil a la foto de hechos. Los días se cuentan en la zona del
        /// usuario: su lunes y su medianoche (D-25).
        /// </summary>
        public static EstadisticasUsuario Snapshot(UsuarioPerfil u)
        {
            ArgumentNullException.ThrowIfNull(u);

            var zona = ZonaHorariaUsuario.De(u);
            var ahora = ZonaHorariaUsuario.Ahora(zona);
            var hoy = DateOnly.FromDateTime(ahora);
            // Semana de calendario: las misiones reinician el lunes, no en ventana móvil.
            int diasDesdeLunes = ((int)ahora.DayOfWeek + 6) % 7;   // lunes=0 … domingo=6
            var desde = ahora.Date.AddDays(-diasDesdeLunes);

            var fechasEntreno = u.EntrenamientosCompletados
                .Select(e => ZonaHorariaUsuario.ALocal(e.Fecha, zona))
                .ToList();
            var rachas = CalculadorRachas.Calcular(fechasEntreno, hoy);

            // La fecha del diario no es un instante sino la etiqueta del día elegido
            // (ServicioDiario la guarda como su medianoche): convertirla la corre un día.
            var diasDiario = u.Diario
                .Select(d => DateOnly.FromDateTime(d.Fecha))
                .Distinct()
                .ToList();

            int diasConDiario = diasDiario.Count;
            int diasConDiarioSemana = diasDiario.Count(d => d >= DateOnly.FromDateTime(desde));

            return new EstadisticasUsuario(
                TotalEntrenamientos: u.EntrenamientosCompletados.Count,
                TotalRecords: u.RecordsPersonales.Count,
                DiasConDiario: diasConDiario,
                TotalRegistrosPeso: u.HistorialProgreso.Count,
                RachaActual: rachas.Actual,
                RachaMaxima: rachas.MasLarga,
                TieneObjetivo: u.ObjetivoActual is not null,
                EntrenamientosEstaSemana: fechasEntreno.Count(f => f >= desde),
                RegistrosPesoEstaSemana: u.HistorialProgreso.Count(r => ZonaHorariaUsuario.ALocal(r.Fecha, zona) >= desde),
                DiasConDiarioEstaSemana: diasConDiarioSemana);
        }
    }
}
