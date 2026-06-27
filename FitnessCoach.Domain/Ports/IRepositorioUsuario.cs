using FitnessCoach.Domain.Models;
namespace FitnessCoach.Domain.Ports
{
    public interface IRepositorioUsuario
    {
        UsuarioPerfil? ObtenerPorId(int id);
        void Guardar(UsuarioPerfil usuario);
    }
}
