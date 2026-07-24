using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Infrastructure.Data;

namespace FitnessCoach.Infrastructure.Repositories
{

    public class RepositorioUsuarioSql : IRepositorioUsuario
    {
        private readonly ApplicationDbContext _context;

        public RepositorioUsuarioSql(ApplicationDbContext context)
        {
            _context = context;
        }

        public UsuarioPerfil? ObtenerPorId(int id)
        {
            return _context.UsuariosPerfil
                .FirstOrDefault(u => u.Id == id);
        }

        public void Guardar(UsuarioPerfil usuario)
        {
            var yaRastreada = usuario.Id != 0
                && _context.ChangeTracker.Entries<UsuarioPerfil>()
                    .Any(e => e.Entity.Id == usuario.Id);

            if (yaRastreada)
            {
                _context.SaveChanges();
                return;
            }

            if (usuario.Id == 0)
            {
                _context.UsuariosPerfil.Add(usuario);
                _context.SaveChanges();
                return;
            }

            var existente = _context.UsuariosPerfil.FirstOrDefault(u => u.Id == usuario.Id);
            if (existente == null)
            {
                _context.UsuariosPerfil.Add(usuario);
            }
            else
            {

                _context.Entry(existente).CurrentValues.SetValues(usuario);
            }

            _context.SaveChanges();
        }
    }
}
