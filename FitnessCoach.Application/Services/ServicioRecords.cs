using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public class ServicioRecords : IServicioRecords
    {
        private readonly IServicioPerfilUsuario _perfiles;

        public ServicioRecords(IServicioPerfilUsuario perfiles)
        {
            _perfiles = perfiles;
        }

        public IReadOnlyList<RecordPersonal> ObtenerTodos(string identityUserId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);
            return usuario.RecordsPersonales.OrderByDescending(r => r.Fecha).ToList();
        }

        public RecordPersonal? ObtenerDeEjercicio(string identityUserId, string ejercicioSlug)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);
            return usuario.RecordsPersonales.FirstOrDefault(r => r.EjercicioSlug == ejercicioSlug);
        }

        public ResultadoRecord Registrar(string identityUserId, string ejercicioSlug, string ejercicioNombre,
                                         double pesoKg, int repeticiones)
        {
            if (string.IsNullOrWhiteSpace(ejercicioSlug))
                throw new ArgumentException("Se requiere el ejercicio.", nameof(ejercicioSlug));

            var usuario = _perfiles.ObtenerOCrear(identityUserId);
            var anterior = usuario.RecordsPersonales.FirstOrDefault(r => r.EjercicioSlug == ejercicioSlug);

            // Primera marca de este ejercicio: siempre es récord.
            if (anterior is null)
            {
                var nuevo = new RecordPersonal
                {
                    EjercicioSlug = ejercicioSlug,
                    EjercicioNombre = ejercicioNombre,
                    PesoKg = pesoKg,
                    Repeticiones = repeticiones,
                    Fecha = DateTime.UtcNow
                };

                usuario.RecordsPersonales.Add(nuevo);
                _perfiles.Guardar(usuario);
                return new ResultadoRecord(EsNuevoRecord: true, nuevo, MejoraKg: null);
            }

            // Se compara por peso, que es como se lee un récord de fuerza ("levanté 100").
            // A igual peso, gana quien haga más repeticiones.
            bool superaElAnterior = pesoKg > anterior.PesoKg
                || (pesoKg == anterior.PesoKg && repeticiones > anterior.Repeticiones);

            if (!superaElAnterior)
                return new ResultadoRecord(EsNuevoRecord: false, anterior, MejoraKg: null);

            var mejora = Math.Round(pesoKg - anterior.PesoKg, 2);

            anterior.PesoKg = pesoKg;
            anterior.Repeticiones = repeticiones;
            anterior.EjercicioNombre = ejercicioNombre;
            anterior.Fecha = DateTime.UtcNow;

            _perfiles.Guardar(usuario);
            return new ResultadoRecord(EsNuevoRecord: true, anterior, mejora);
        }

        public bool Eliminar(string identityUserId, int recordId)
        {
            var usuario = _perfiles.ObtenerOCrear(identityUserId);

            var record = usuario.RecordsPersonales.FirstOrDefault(r => r.Id == recordId);
            if (record is null) return false;

            usuario.RecordsPersonales.Remove(record);
            _perfiles.Guardar(usuario);
            return true;
        }
    }
}
