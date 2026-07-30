using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Recuerda el perfil ya leído: el servicio es scoped, así que la memoria dura una
    /// petición. Una pantalla lo pedía hasta seis veces, una por servicio.
    /// </summary>
    public class ServicioPerfilUsuario : IServicioPerfilUsuario
    {
        private readonly IRepositorioUsuario _repositorio;

        private readonly Dictionary<string, UsuarioPerfil> _yaLeidos = new(StringComparer.Ordinal);

        public ServicioPerfilUsuario(IRepositorioUsuario repositorio)
        {
            _repositorio = repositorio;
        }

        public UsuarioPerfil ObtenerOCrear(string identityUserId)
        {
            if (string.IsNullOrWhiteSpace(identityUserId))
                throw new ArgumentException("Se requiere el identityUserId.", nameof(identityUserId));

            if (_yaLeidos.TryGetValue(identityUserId, out var recordado))
                return recordado;

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

            _yaLeidos[identityUserId] = perfil;
            return perfil;
        }

        public UsuarioPerfil? Obtener(string identityUserId)
        {
            if (string.IsNullOrWhiteSpace(identityUserId))
                throw new ArgumentException("Se requiere el identityUserId.", nameof(identityUserId));

            if (_yaLeidos.TryGetValue(identityUserId, out var recordado))
                return recordado;

            var perfil = _repositorio.ObtenerPorIdentityUserId(identityUserId);
            if (perfil is not null) _yaLeidos[identityUserId] = perfil;

            return perfil;
        }

        public void Guardar(UsuarioPerfil usuario)
        {
            _repositorio.Guardar(usuario);

            // Esta instancia pasa a ser la versión vigente del perfil.
            if (!string.IsNullOrWhiteSpace(usuario?.IdentityUserId))
                _yaLeidos[usuario.IdentityUserId] = usuario;
        }
    }
}
