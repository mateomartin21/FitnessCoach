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
        /// Los días de la rutina real del usuario, como etiquetas ("Día 1 — Piernas"). La
        /// rutina es determinista (mismo objetivo y misma semilla, mismos días), así que
        /// regenerarla acá coincide con lo que el usuario ve en su pantalla.
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

            // Solo se puede marcar como hecho un día REAL de la propia rutina. Si fuera
            // texto libre, cualquiera anota algo inventado y se lleva el XP y los logros sin
            // haber entrenado. La regla vive acá, no en el controlador, para que la cumplan
            // por igual la pantalla y la API (D-26).
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

            // Las fechas se guardan en UTC pero la racha es un concepto de calendario:
            // "entrené hoy" depende de la medianoche DEL USUARIO, no la de UTC ni la del
            // servidor. Por eso todo se traduce a su zona antes de contar días (D-25).
            var zona = ZonaHorariaUsuario.De(usuario);
            var fechasLocales = usuario.EntrenamientosCompletados
                .Select(e => ZonaHorariaUsuario.ALocal(e.Fecha, zona));

            return CalculadorRachas.Calcular(fechasLocales, ZonaHorariaUsuario.Hoy(zona));
        }
    }
}
