using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public class ServicioEntrenamientos : IServicioEntrenamientos
    {
        private readonly IServicioPerfilUsuario _perfiles;

        public ServicioEntrenamientos(IServicioPerfilUsuario perfiles)
        {
            _perfiles = perfiles;
        }

        public IReadOnlyList<EntrenamientoCompletado> ObtenerHistorial(string identityUserId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);
            return usuario.EntrenamientosCompletados.OrderByDescending(e => e.Fecha).ToList();
        }

        public EntrenamientoCompletado Registrar(string identityUserId, string nombreRutina, int duracionMinutos, string? notas)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);

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
            // "entrené hoy" depende de la medianoche del usuario, no la de UTC. Se cuenta
            // en la hora local del servidor, que es una aproximación — ver D-25.
            var fechasLocales = usuario.EntrenamientosCompletados.Select(e => e.Fecha.ToLocalTime());
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            return CalculadorRachas.Calcular(fechasLocales, hoy);
        }
    }
}
