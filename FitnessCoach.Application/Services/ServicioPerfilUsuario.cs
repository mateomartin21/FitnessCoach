using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Punto único de acceso al perfil del usuario de la petición.
    ///
    /// Recuerda lo que ya leyó (el servicio es scoped: vive UNA petición HTTP). Sin esto, una
    /// sola pantalla lo pedía media docena de veces —el controlador, más cada servicio que
    /// arranca de su propio <c>ObtenerOCrear</c>— y cada pedido era un viaje a SQL.
    /// No cambia lo que se ve: EF ya devolvía la misma instancia rastreada en todas esas
    /// lecturas, así que recordarla solo ahorra los viajes.
    /// </summary>
    public class ServicioPerfilUsuario : IServicioPerfilUsuario
    {
        private readonly IRepositorioUsuario _repositorio;

        /// <summary>Perfiles ya leídos en esta petición, por identidad.</summary>
        private readonly Dictionary<string, UsuarioPerfil> _yaLeidos = new(StringComparer.Ordinal);

        public ServicioPerfilUsuario(IRepositorioUsuario repositorio)
        {
            _repositorio = repositorio;
        }

        // Trae el perfil del usuario; si es su primera vez, le crea uno por defecto.
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

            // Tras guardar, esta instancia es la versión vigente del perfil: se recuerda para
            // que lo que siga en la misma petición no vuelva a leer de la base.
            if (!string.IsNullOrWhiteSpace(usuario?.IdentityUserId))
                _yaLeidos[usuario.IdentityUserId] = usuario;
        }
    }
}
