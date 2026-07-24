using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public interface IServicioPerfilUsuario
    {
        UsuarioPerfil ObtenerOCrear(string identityUserId);
        UsuarioPerfil? Obtener(string identityUserId);
        void Guardar(UsuarioPerfil usuario);
    }
}
