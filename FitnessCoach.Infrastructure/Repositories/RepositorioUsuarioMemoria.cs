using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Infrastructure.Repositoriess
{
    public class RepositorioUsuarioMemoria : IRepositorioUsuario
    {
        private readonly List<UsuarioPerfil> _usuarios = new();

        public UsuarioPerfil? ObtenerPorId(int id)
        {
            return _usuarios.FirstOrDefault(u => u.Id == id);
        }
        public void Guardar(UsuarioPerfil usuario)
        {
            if (usuario.Id == 0)
            {
                // Es un usuario nuevo
                usuario.Id = _usuarios.Count + 1;
                _usuarios.Add(usuario);
            }
            else
            {
                // Es un usuario que ya existe, vamos a buscarlo para actualizarlo
                var usuarioExistente = _usuarios.FirstOrDefault(u => u.Id == usuario.Id);

                if (usuarioExistente != null)
                {
                    // Sobrescribimos los datos viejos con los nuevos que vienen del formulario
                    usuarioExistente.Nombre = usuario.Nombre;
                    usuarioExistente.Edad = usuario.Edad;
                    usuarioExistente.PesoKg = usuario.PesoKg;
                    usuarioExistente.EstaturaCm = usuario.EstaturaCm;
                    usuarioExistente.ObjetivoActual = usuario.ObjetivoActual;
                }
                else
                {
                    // Por si acaso le pasamos un ID que no estaba en la lista
                    _usuarios.Add(usuario);
                }
            }
        }
    }
}
