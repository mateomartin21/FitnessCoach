using FitnessCoach.Domain.Models;
namespace FitnessCoach.Domain.Ports
{
    public interface IRepositorioUsuario
    {
        UsuarioPerfil? ObtenerPorId(int id);
        UsuarioPerfil? ObtenerPorIdentityUserId(string identityUserId);
        void Guardar(UsuarioPerfil usuario);
    }
}
