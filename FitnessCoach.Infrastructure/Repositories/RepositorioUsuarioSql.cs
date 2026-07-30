using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            return PerfilCompleto.FirstOrDefault(u => u.Id == id);
        }

        public UsuarioPerfil? ObtenerPorIdentityUserId(string identityUserId)
        {
            return PerfilCompleto.FirstOrDefault(u => u.IdentityUserId == identityUserId);
        }

        /// <summary>
        /// El perfil trae cuatro colecciones owned (diario, entrenamientos, historial de peso
        /// y récords) y EF las incluye siempre. En una sola sentencia eso son cuatro LEFT JOIN
        /// entre colecciones que no tienen relación entre sí, o sea un producto cartesiano:
        /// con 100 comidas, 50 entrenamientos, 50 pesos y 10 récords, SQL Server devuelve
        /// 2.5 millones de filas para leer UN perfil. AsSplitQuery pide una sentencia por
        /// colección (cinco en total) y cada una devuelve solo sus filas.
        /// </summary>
        private IQueryable<UsuarioPerfil> PerfilCompleto => _context.UsuariosPerfil.AsSplitQuery();


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

            var existente = PerfilCompleto.FirstOrDefault(u => u.Id == usuario.Id);
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
