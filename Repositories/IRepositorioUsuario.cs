using FitnessCoach.Models;

namespace FitnessCoach.Repositories
{
    public interface IRepositorioUsuario
    {
        UsuarioPerfil? ObtenerPorId(int id);
        void Guardar(UsuarioPerfil usuario);
    }
}