using FitnessCoach.Domain.Models.Coaching;

namespace FitnessCoach.Domain.Ports
{
    /// <summary>
    /// Dónde se guarda la charla con Koda. Puerto aparte del de usuario a propósito:
    /// la conversación se lee solo cuando se abre el chat, no en cada pantalla
    /// (ver la nota de <see cref="MensajeCoach"/>).
    /// </summary>
    public interface IRepositorioConversacion
    {
        /// <summary>Los últimos <paramref name="cantidad"/> mensajes, del más viejo al más nuevo.</summary>
        IReadOnlyList<MensajeCoach> Ultimos(int usuarioPerfilId, int cantidad);

        void Agregar(IEnumerable<MensajeCoach> mensajes);

        /// <summary>Borra los más viejos y deja como mucho <paramref name="conservar"/>.</summary>
        void Podar(int usuarioPerfilId, int conservar);

        void BorrarTodo(int usuarioPerfilId);
    }
}
