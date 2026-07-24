using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Reglas del historial de peso. Vive acá y no en el controlador porque son decisiones
    /// del negocio (qué peso "vale" tras editar o borrar), y porque acá se pueden probar
    /// sin levantar HTTP ni base de datos.
    /// Toda operación arranca del perfil del usuario autenticado: un registro ajeno
    /// nunca aparece, así que no hay id que manipular (misma idea del ADR-10).
    /// </summary>
    public class ServicioProgreso : IServicioProgreso
    {
        private readonly IServicioPerfilUsuario _perfiles;

        public ServicioProgreso(IServicioPerfilUsuario perfiles)
        {
            _perfiles = perfiles;
        }

        public IReadOnlyList<RegistroProgreso> ObtenerHistorial(string identityUserId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);
            return usuario.HistorialProgreso.OrderByDescending(r => r.Fecha).ToList();
        }

        public RegistroProgreso? ObtenerRegistro(string identityUserId, int registroId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);
            return usuario.HistorialProgreso.FirstOrDefault(r => r.Id == registroId);
        }

        public RegistroProgreso Agregar(string identityUserId, double pesoKg, string? notas)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);

            var registro = new RegistroProgreso
            {
                Fecha = DateTime.UtcNow,   // la fecha la pone el servidor, siempre en UTC (D-10)
                PesoKg = pesoKg,
                Notas = notas ?? string.Empty
            };

            usuario.HistorialProgreso.Add(registro);
            usuario.PesoKg = pesoKg;

            _perfiles.Guardar(usuario);
            return registro;
        }

        public bool Editar(string identityUserId, int registroId, double pesoKg, string? notas)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);

            var registro = usuario.HistorialProgreso.FirstOrDefault(r => r.Id == registroId);
            if (registro is null) return false;

            registro.PesoKg = pesoKg;
            registro.Notas = notas ?? string.Empty;

            SincronizarPesoDelPerfil(usuario);

            _perfiles.Guardar(usuario);
            return true;
        }

        public bool Eliminar(string identityUserId, int registroId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);

            var registro = usuario.HistorialProgreso.FirstOrDefault(r => r.Id == registroId);
            if (registro is null) return false;

            usuario.HistorialProgreso.Remove(registro);
            SincronizarPesoDelPerfil(usuario);

            _perfiles.Guardar(usuario);
            return true;
        }

        /// <summary>
        /// El peso del perfil refleja el registro más reciente que exista. Si se borró el
        /// último registro que quedaba, se conserva el peso anterior: dejarlo en 0 pondría
        /// al perfil fuera del rango válido y rompería el cálculo calórico.
        /// </summary>
        private static void SincronizarPesoDelPerfil(UsuarioPerfil usuario)
        {
            var masReciente = usuario.HistorialProgreso
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefault();

            if (masReciente is not null)
                usuario.PesoKg = masReciente.PesoKg;
        }
    }
}
