using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Services
{
    public class ServicioPerfilUsuario : IServicioPerfilUsuario
    {
        private readonly IRepositorioUsuario _repositorio;

        public ServicioPerfilUsuario(IRepositorioUsuario repositorio)
        {
            _repositorio = repositorio;
        }

        // Trae el perfil del usuario; si es su primera vez, le crea uno por defecto.
        public UsuarioPerfil ObtenerOCrear(string identityUserId)
        {
            if (string.IsNullOrWhiteSpace(identityUserId))
                throw new ArgumentException("Se requiere el identityUserId.", nameof(identityUserId));

            var perfil = _repositorio.ObtenerPorIdentityUserId(identityUserId);
            if (perfil is null)
            {
                perfil = new UsuarioPerfil
                {
                    IdentityUserId = identityUserId,
                    Nombre = "Nuevo usuario",
                    Edad = 25,
                    EstaturaCm = 170,
                    PesoKg = 70,
                    ObjetivoActual = new ObjetivoRecomposicion()
                };
                _repositorio.Guardar(perfil);
            }
            return perfil;
        }

        public UsuarioPerfil? Obtener(string identityUserId)
        {
            if (string.IsNullOrWhiteSpace(identityUserId))
                throw new ArgumentException("Se requiere el identityUserId.", nameof(identityUserId));

            return _repositorio.ObtenerPorIdentityUserId(identityUserId);
        }

        public void Guardar(UsuarioPerfil usuario) => _repositorio.Guardar(usuario);
    }
}
