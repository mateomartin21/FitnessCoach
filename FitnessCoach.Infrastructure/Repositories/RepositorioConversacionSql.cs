using FitnessCoach.Domain.Models.Coaching;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCoach.Infrastructure.Repositories
{
    /// <summary>
    /// La charla con Koda, en la base. El Id es autoincremental, así que ordenar por Id
    /// es ordenar cronológicamente sin depender de la precisión del reloj: dos mensajes
    /// del mismo intercambio pueden compartir la marca de tiempo al milisegundo.
    /// </summary>
    public class RepositorioConversacionSql : IRepositorioConversacion
    {
        private readonly ApplicationDbContext _context;

        public RepositorioConversacionSql(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IReadOnlyList<MensajeCoach> Ultimos(int usuarioPerfilId, int cantidad)
        {
            if (cantidad <= 0) return Array.Empty<MensajeCoach>();

            // Se piden los ULTIMOS (descendente) y despues se da vuelta en memoria: al
            // reves habria que traer la conversacion entera para quedarse con el final.
            var ultimos = _context.MensajesCoach
                .AsNoTracking()
                .Where(m => m.UsuarioPerfilId == usuarioPerfilId)
                .OrderByDescending(m => m.Id)
                .Take(cantidad)
                .ToList();

            ultimos.Reverse();
            return ultimos;
        }

        public void Agregar(IEnumerable<MensajeCoach> mensajes)
        {
            ArgumentNullException.ThrowIfNull(mensajes);

            var nuevos = mensajes.ToList();
            if (nuevos.Count == 0) return;

            _context.MensajesCoach.AddRange(nuevos);
            _context.SaveChanges();
        }

        public void Podar(int usuarioPerfilId, int conservar)
        {
            if (conservar < 0) conservar = 0;

            // Una sola sentencia contra la base: sin ExecuteDelete habria que materializar
            // las filas sobrantes solo para volver a mandarlas como DELETE.
            var corte = _context.MensajesCoach
                .Where(m => m.UsuarioPerfilId == usuarioPerfilId)
                .OrderByDescending(m => m.Id)
                .Skip(conservar)
                .Select(m => (int?)m.Id)
                .FirstOrDefault();

            if (corte is null) return;   // todavia no hay de mas

            _context.MensajesCoach
                .Where(m => m.UsuarioPerfilId == usuarioPerfilId && m.Id <= corte)
                .ExecuteDelete();
        }

        public void BorrarTodo(int usuarioPerfilId)
        {
            _context.MensajesCoach
                .Where(m => m.UsuarioPerfilId == usuarioPerfilId)
                .ExecuteDelete();
        }
    }
}
