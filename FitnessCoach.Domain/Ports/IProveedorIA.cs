namespace FitnessCoach.Domain.Ports
{
    /// <summary>
    /// Un proveedor de respuestas de IA para el coach. Es un puerto genérico: no sabe
    /// nada del Lobo ni de cómo se arma el prompt, solo "te doy una consulta, me das
    /// una respuesta o fallás".
    ///
    /// El contrato de fallo es explícito: ante cualquier problema (red caída, la API
    /// devuelve un error, la respuesta viene malformada) el proveedor **lanza**
    /// <see cref="CoachIAException"/>. No devuelve el error como si fuera una respuesta
    /// válida — ese era justamente el bug que hacía imposible el fallback (D-09).
    /// </summary>
    public interface IProveedorIA
    {
        /// <summary>Nombre corto del proveedor, para los logs ("Gemini", "Offline").</summary>
        string Nombre { get; }

        /// <summary>
        /// Si es un proveedor de respaldo (una respuesta degradada, no la IA real).
        /// La cadena lo usa para avisar que la respuesta no vino del coach inteligente.
        /// </summary>
        bool EsRespaldo { get; }

        /// <summary>
        /// Genera una respuesta para la consulta. Devuelve el texto o lanza
        /// <see cref="CoachIAException"/> si no pudo.
        /// </summary>
        Task<string> GenerarAsync(ConsultaIA consulta, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Lo que se le pide a un proveedor. Lleva el prompt ya armado (para los proveedores
    /// que hablan con un modelo de lenguaje) y también el mensaje y el perfil por
    /// separado (para un proveedor sin IA que razona por reglas). Así la personalidad
    /// del Lobo se arma en un solo lugar y no se repite en cada proveedor.
    /// </summary>
    public sealed record ConsultaIA(string Prompt, string Mensaje, string ContextoPerfil);
}
