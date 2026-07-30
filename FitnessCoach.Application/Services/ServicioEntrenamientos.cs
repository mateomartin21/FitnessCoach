using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public class ServicioEntrenamientos : IServicioEntrenamientos
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly IGeneradorRutinas _rutinas;

        public ServicioEntrenamientos(IServicioPerfilUsuario perfiles, IGeneradorRutinas rutinas)
        {
            _perfiles = perfiles;
            _rutinas = rutinas;
        }

        public IReadOnlyList<EntrenamientoCompletado> ObtenerHistorial(string identityUserId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);
            return usuario.EntrenamientosCompletados.OrderByDescending(e => e.Fecha).ToList();
        }

        /// <summary>
        /// La rutina es determinista, así que regenerarla acá da los mismos días que ve el usuario.
        /// </summary>
        public IReadOnlyList<string> OpcionesDeRutina(string identityUserId) =>
            OpcionesDe(_perfiles.ObtenerOCrear(identityUserId));

        private IReadOnlyList<string> OpcionesDe(UsuarioPerfil usuario)
        {
            if (usuario.ObjetivoActual is null) return Array.Empty<string>();

            var rutina = _rutinas.GenerarRutinaParaObjetivo(usuario.ObjetivoActual, usuario.Id);
            return rutina.Dias.Select(d => $"{d.NombreDia} — {d.Enfoque}").ToList();
        }

        public EntrenamientoCompletado Registrar(string identityUserId, string nombreRutina, int duracionMinutos, string? notas)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);

            // Con texto libre cualquiera se lleva XP y logros sin entrenar. La regla vive
            // en el servicio para que la cumplan la pantalla y la API por igual (D-26).
            if (!OpcionesDe(usuario).Contains(nombreRutina))
                throw new ArgumentException(
                    "El entrenamiento tiene que ser uno de los días de tu rutina.", nameof(nombreRutina));

            var entrenamiento = new EntrenamientoCompletado
            {
                Fecha = DateTime.UtcNow,   // la fecha la pone el servidor, siempre en UTC (D-10)
                NombreRutina = nombreRutina,
                DuracionMinutos = duracionMinutos,
                Notas = notas ?? string.Empty
            };

            usuario.EntrenamientosCompletados.Add(entrenamiento);
            _perfiles.Guardar(usuario);

            return entrenamiento;
        }

        public bool Eliminar(string identityUserId, int entrenamientoId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);

            var entrenamiento = usuario.EntrenamientosCompletados.FirstOrDefault(e => e.Id == entrenamientoId);
            if (entrenamiento is null) return false;

            usuario.EntrenamientosCompletados.Remove(entrenamiento);
            _perfiles.Guardar(usuario);
            return true;
        }

        public Rachas ObtenerRachas(string identityUserId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);

            // "Entrené hoy" depende de la medianoche del usuario, no de la de UTC (D-25).
            var zona = ZonaHorariaUsuario.De(usuario);
            var fechasLocales = usuario.EntrenamientosCompletados
                .Select(e => ZonaHorariaUsuario.ALocal(e.Fecha, zona));

            return CalculadorRachas.Calcular(fechasLocales, ZonaHorariaUsuario.Hoy(zona));
        }
    }
}
