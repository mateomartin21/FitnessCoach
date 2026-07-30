using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Tests.Fakes
{
    /// <summary>
    /// Implementación de mentira del puerto IRepositorioUsuario, en memoria y sin EF.
    /// Nos deja probar ServicioPerfilUsuario sin levantar una base de datos (ADR-08).
    /// Cuenta las escrituras para poder afirmar que el servicio NO guarda de más.
    /// </summary>
    public class RepositorioUsuarioFalso : IRepositorioUsuario
    {
        private readonly List<UsuarioPerfil> _usuarios = new();
        private int _siguienteId = 1;

        /// <summary>Cuántas veces se llamó a Guardar.</summary>
        public int VecesQueSeGuardo { get; private set; }

        /// <summary>Cuántas lecturas por identidad: contra la base real, cada una es un viaje a SQL.</summary>
        public int VecesQueSeBuscoPorIdentidad { get; private set; }

        public RepositorioUsuarioFalso(params UsuarioPerfil[] usuariosIniciales)
        {
            foreach (var usuario in usuariosIniciales)
            {
                if (usuario.Id == 0) usuario.Id = _siguienteId++;
                _usuarios.Add(usuario);
            }
        }

        public UsuarioPerfil? ObtenerPorId(int id) =>
            _usuarios.FirstOrDefault(u => u.Id == id);

        public UsuarioPerfil? ObtenerPorIdentityUserId(string identityUserId)
        {
            VecesQueSeBuscoPorIdentidad++;
            return _usuarios.FirstOrDefault(u => u.IdentityUserId == identityUserId);
        }

        public void Guardar(UsuarioPerfil usuario)
        {
            VecesQueSeGuardo++;

            if (usuario.Id == 0)
            {
                usuario.Id = _siguienteId++;
                _usuarios.Add(usuario);
                return;
            }

            // Ya estaba: lo damos por actualizado en el sitio, como haría EF con una entidad rastreada.
            if (!_usuarios.Contains(usuario))
                _usuarios.Add(usuario);
        }
    }
}
