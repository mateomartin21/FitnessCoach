using FitnessCoach.Domain.Models.Coaching;

namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// El caso de uso "preguntarle al Lobo Coach". Es lo que consume el controlador, en
    /// vez de depender de un adaptador de IA concreto como hacía antes.
    /// </summary>
    public interface ICoachIA
    {
        /// <param name="historial">
        /// Los últimos turnos de la charla, para que Koda retome el hilo. Opcional: las
        /// tarjetas de análisis son consultas sueltas y no llevan conversación detrás.
        /// </param>
        Task<RespuestaCoach> ConsultarAsync(
            string mensaje,
            string contextoPerfil,
            IReadOnlyList<MensajeCoach>? historial = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// La respuesta del coach, con de dónde salió. Nunca es un error crudo: si la IA
    /// falló, <see cref="EsDegradada"/> es true y el texto es lo que dice el Lobo cuando
    /// se le va la señal.
    /// </summary>
    /// <param name="Texto">Lo que responde el Lobo.</param>
    /// <param name="Fuente">Qué proveedor respondió ("Gemini", "Offline", "Degradado").</param>
    /// <param name="EsDegradada">Si la respuesta no vino de la IA real, sino de un respaldo.</param>
    public sealed record RespuestaCoach(string Texto, string Fuente, bool EsDegradada);
}
