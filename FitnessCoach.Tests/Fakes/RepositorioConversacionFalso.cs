using FitnessCoach.Domain.Models.Coaching;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Tests.Fakes
{
    /// <summary>
    /// El puerto de conversación en memoria, sin EF (ADR-08). Reproduce lo que importa
    /// del adaptador real: los ids crecen, así que ordenar por id es orden cronológico.
    /// </summary>
    public class RepositorioConversacionFalso : IRepositorioConversacion
    {
        private readonly List<MensajeCoach> _mensajes = new();
        private int _siguienteId = 1;

        /// <summary>Cuántas veces se podó: contra la base real cada una es un DELETE.</summary>
        public int VecesQueSePodo { get; private set; }

        public IReadOnlyList<MensajeCoach> Ultimos(int usuarioPerfilId, int cantidad)
        {
            if (cantidad <= 0) return Array.Empty<MensajeCoach>();

            return _mensajes
                .Where(m => m.UsuarioPerfilId == usuarioPerfilId)
                .OrderByDescending(m => m.Id)
                .Take(cantidad)
                .OrderBy(m => m.Id)
                .ToList();
        }

        public void Agregar(IEnumerable<MensajeCoach> mensajes)
        {
            foreach (var mensaje in mensajes)
            {
                mensaje.Id = _siguienteId++;
                _mensajes.Add(mensaje);
            }
        }

        public void Podar(int usuarioPerfilId, int conservar)
        {
            VecesQueSePodo++;

            var sobrantes = _mensajes
                .Where(m => m.UsuarioPerfilId == usuarioPerfilId)
                .OrderByDescending(m => m.Id)
                .Skip(Math.Max(0, conservar))
                .ToList();

            foreach (var viejo in sobrantes) _mensajes.Remove(viejo);
        }

        public void BorrarTodo(int usuarioPerfilId) =>
            _mensajes.RemoveAll(m => m.UsuarioPerfilId == usuarioPerfilId);

        /// <summary>Todo lo guardado, para afirmar sobre el estado sin pasar por el puerto.</summary>
        public IReadOnlyList<MensajeCoach> Todo => _mensajes;
    }
}
