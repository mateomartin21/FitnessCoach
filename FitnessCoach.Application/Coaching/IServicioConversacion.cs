using FitnessCoach.Domain.Models.Coaching;

namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// El caso de uso "la charla con Koda se queda": leerla al abrir el chat, guardar
    /// cada intercambio y poder empezar de cero.
    /// </summary>
    public interface IServicioConversacion
    {
        /// <summary>Lo que se muestra al abrir el chat, del más viejo al más nuevo.</summary>
        IReadOnlyList<MensajeCoach> Historial(int usuarioPerfilId);

        /// <summary>Lo que Koda recibe como memoria: solo los últimos intercambios.</summary>
        IReadOnlyList<MensajeCoach> Memoria(int usuarioPerfilId);

        /// <summary>Guarda la pregunta y la respuesta, y poda lo que sobra.</summary>
        void RegistrarIntercambio(int usuarioPerfilId, string pregunta, string respuesta);

        void Borrar(int usuarioPerfilId);
    }
}
