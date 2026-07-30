using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Resuelve la zona horaria del usuario y convierte las marcas guardadas (UTC) a su
    /// calendario. Las rachas, misiones y el "hoy" del diario se cuentan acá, no en la
    /// hora del servidor (D-25). Es el único lugar que decide "qué día es para el usuario".
    /// </summary>
    public static class ZonaHorariaUsuario
    {
        /// <summary>Zona por defecto cuando el perfil no tiene una definida.</summary>
        public const string PorDefecto = "America/Mexico_City";

        /// <summary>
        /// Las zonas que ofrece el selector del perfil, en ids IANA. No es la lista completa
        /// (son cientos): son las de México y el resto de Latinoamérica, más las de Estados
        /// Unidos y España, donde puede vivir alguien de acá. Quien necesite otra la recibe
        /// del navegador (el formulario acepta cualquier id válido del sistema).
        /// </summary>
        public static readonly IReadOnlyList<(string Id, string Etiqueta)> Comunes = new[]
        {
            ("America/Mexico_City",            "Ciudad de México (centro)"),
            ("America/Cancun",                 "Cancún / Quintana Roo"),
            ("America/Monterrey",              "Monterrey"),
            ("America/Mazatlan",               "Mazatlán / Sinaloa"),
            ("America/Chihuahua",              "Chihuahua"),
            ("America/Hermosillo",             "Hermosillo / Sonora"),
            ("America/Tijuana",                "Tijuana / Baja California"),
            ("America/Guatemala",              "Guatemala / El Salvador"),
            ("America/Bogota",                 "Bogotá / Lima / Quito"),
            ("America/Caracas",                "Caracas"),
            ("America/Santiago",               "Santiago de Chile"),
            ("America/Argentina/Buenos_Aires", "Buenos Aires / Montevideo"),
            ("America/Sao_Paulo",              "São Paulo"),
            ("America/Los_Angeles",            "Los Ángeles (Pacífico EUA)"),
            ("America/Denver",                 "Denver (Montaña EUA)"),
            ("America/Chicago",                "Chicago (Centro EUA)"),
            ("America/New_York",               "Nueva York (Este EUA)"),
            ("Europe/Madrid",                  "Madrid"),
            ("UTC",                            "UTC (hora universal)"),
        };

        /// <summary>
        /// Devuelve la <see cref="TimeZoneInfo"/> del id dado (IANA o Windows; .NET convierte
        /// entre ambos). Cae a la zona por defecto y, en último caso, a UTC, sin lanzar.
        /// </summary>
        public static TimeZoneInfo Resolver(string? id)
        {
            foreach (var candidato in new[] { id, PorDefecto })
            {
                if (string.IsNullOrWhiteSpace(candidato)) continue;
                try { return TimeZoneInfo.FindSystemTimeZoneById(candidato); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.Utc;
        }

        /// <summary>La zona del perfil.</summary>
        public static TimeZoneInfo De(UsuarioPerfil usuario) => Resolver(usuario?.ZonaHoraria);

        /// <summary>Indica si un id de zona es válido en este sistema.</summary>
        public static bool EsValida(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try { TimeZoneInfo.FindSystemTimeZoneById(id); return true; }
            catch (TimeZoneNotFoundException) { return false; }
            catch (InvalidTimeZoneException) { return false; }
        }

        /// <summary>"Ahora" en la zona del usuario (Kind Unspecified: es hora de pared).</summary>
        public static DateTime Ahora(TimeZoneInfo zona) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona);

        /// <summary>El día de calendario del usuario en este momento.</summary>
        public static DateOnly Hoy(TimeZoneInfo zona) => DateOnly.FromDateTime(Ahora(zona));

        /// <summary>
        /// Convierte una marca guardada a la hora de pared del usuario. Las fechas se guardan
        /// en UTC pero EF las devuelve con Kind sin especificar; acá se tratan como UTC.
        /// </summary>
        public static DateTime ALocal(DateTime fechaGuardada, TimeZoneInfo zona) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(fechaGuardada, DateTimeKind.Utc), zona);
    }
}
