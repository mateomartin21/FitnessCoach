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
        /// Traduce el perfil a la foto de hechos. Las fechas se guardan en UTC pero las
        /// rachas y "esta semana" son de calendario, así que se cuentan en hora local del
        /// servidor, igual que el resto del tracker (aproximación conocida, D-25).
        /// </summary>
        public static EstadisticasUsuario Snapshot(UsuarioPerfil u)
        {
            ArgumentNullException.ThrowIfNull(u);

            var ahora = DateTime.Now;
            var hoy = DateOnly.FromDateTime(ahora);
            // Las misiones son de SEMANA DE CALENDARIO: se reinician todas juntas el
            // lunes a las 00:00 (hora local), no en una ventana móvil de 7 días.
            int diasDesdeLunes = ((int)ahora.DayOfWeek + 6) % 7;   // lunes=0 … domingo=6
            var desde = ahora.Date.AddDays(-diasDesdeLunes);

            var fechasEntreno = u.EntrenamientosCompletados
                .Select(e => e.Fecha.ToLocalTime())
                .ToList();
            var rachas = CalculadorRachas.Calcular(fechasEntreno, hoy);

            int diasConDiario = u.Diario
                .Select(d => DateOnly.FromDateTime(d.Fecha.ToLocalTime()))
                .Distinct().Count();

            int diasConDiarioSemana = u.Diario
                .Where(d => d.Fecha.ToLocalTime() >= desde)
                .Select(d => DateOnly.FromDateTime(d.Fecha.ToLocalTime()))
                .Distinct().Count();

            return new EstadisticasUsuario(
                TotalEntrenamientos: u.EntrenamientosCompletados.Count,
                TotalRecords: u.RecordsPersonales.Count,
                DiasConDiario: diasConDiario,
                TotalRegistrosPeso: u.HistorialProgreso.Count,
                RachaActual: rachas.Actual,
                RachaMaxima: rachas.MasLarga,
                TieneObjetivo: u.ObjetivoActual is not null,
                EntrenamientosEstaSemana: fechasEntreno.Count(f => f >= desde),
                RegistrosPesoEstaSemana: u.HistorialProgreso.Count(r => r.Fecha.ToLocalTime() >= desde),
                DiasConDiarioEstaSemana: diasConDiarioSemana);
        }
    }
}
