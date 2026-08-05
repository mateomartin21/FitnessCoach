using FitnessCoach.Domain.Models.Coaching;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Coaching
{
    /// <summary>
    /// Guarda y recupera la charla con Koda. Los topes (cuánto se conserva y cuánto
    /// viaja como memoria) son del dominio, no del adaptador: acá se aplican.
    /// </summary>
    public class ServicioConversacion : IServicioConversacion
    {
        private readonly IRepositorioConversacion _conversaciones;

        public ServicioConversacion(IRepositorioConversacion conversaciones)
        {
            _conversaciones = conversaciones ?? throw new ArgumentNullException(nameof(conversaciones));
        }

        public IReadOnlyList<MensajeCoach> Historial(int usuarioPerfilId) =>
            _conversaciones.Ultimos(usuarioPerfilId, MensajeCoach.MaximoGuardados);

        public IReadOnlyList<MensajeCoach> Memoria(int usuarioPerfilId) =>
            _conversaciones.Ultimos(usuarioPerfilId, MensajeCoach.MensajesDeMemoria);

        public void RegistrarIntercambio(int usuarioPerfilId, string pregunta, string respuesta)
        {
            // Un intercambio a medias no se guarda: un globo del usuario sin respuesta
            // (o al reves) al recargar la pagina se lee como que la app perdio algo.
            if (string.IsNullOrWhiteSpace(pregunta) || string.IsNullOrWhiteSpace(respuesta))
                return;

            var ahora = DateTime.UtcNow;

            _conversaciones.Agregar(new[]
            {
                Con(usuarioPerfilId, MensajeCoach.DelUsuario(pregunta, ahora)),
                Con(usuarioPerfilId, MensajeCoach.DeKoda(respuesta, ahora)),
            });

            _conversaciones.Podar(usuarioPerfilId, MensajeCoach.MaximoGuardados);
        }

        public void Borrar(int usuarioPerfilId) => _conversaciones.BorrarTodo(usuarioPerfilId);

        private static MensajeCoach Con(int usuarioPerfilId, MensajeCoach mensaje)
        {
            mensaje.UsuarioPerfilId = usuarioPerfilId;
            return mensaje;
        }
    }
}
