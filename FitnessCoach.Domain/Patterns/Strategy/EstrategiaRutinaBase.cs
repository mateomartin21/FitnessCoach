using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Domain.Patterns.Strategy
{
    /// <summary>
    /// Compone una rutina tomando ejercicios del catálogo según la plantilla que
    /// declara cada estrategia concreta. Las estrategias dejan de contener ejercicios:
    /// solo dicen qué grupos trabajar, cuántos ejercicios y con qué prescripción.
    ///
    /// La selección es **determinista para una misma semilla**: el mismo usuario ve
    /// siempre su rutina y dos usuarios distintos ven ejercicios distintos. Si fuera
    /// aleatoria en cada carga, la rutina cambiaría al refrescar la página.
    /// </summary>
    public abstract class EstrategiaRutinaBase : IEstrategiaRutina
    {
        private readonly IRepositorioEjercicios _catalogo;
        private readonly int _semillaRotacion;

        protected EstrategiaRutinaBase(IRepositorioEjercicios catalogo, int semillaRotacion = 0)
        {
            _catalogo = catalogo ?? throw new ArgumentNullException(nameof(catalogo));
            _semillaRotacion = semillaRotacion;
        }

        protected abstract string NombreRutina { get; }
        protected abstract string Nivel { get; }
        protected abstract IReadOnlyList<PlantillaDia> Plan { get; }

        /// <summary>
        /// Equipos aceptables, en orden de preferencia. Permite que la rutina de
        /// principiante evite máquinas y la avanzada priorice barra.
        /// Vacío = cualquier equipo.
        /// </summary>
        protected virtual IReadOnlyList<string> EquiposPreferidos => Array.Empty<string>();

        public Rutina GenerarRutina()
        {
            var rutina = new Rutina { NombreRutina = NombreRutina, Nivel = Nivel };

            foreach (var plantilla in Plan)
            {
                var dia = new DiaEntrenamiento
                {
                    NombreDia = plantilla.NombreDia,
                    Enfoque = plantilla.Enfoque
                };

                var yaUsados = new HashSet<string>();

                foreach (var bloque in plantilla.Bloques)
                {
                    foreach (var ejercicio in Elegir(bloque, yaUsados))
                    {
                        yaUsados.Add(ejercicio.Slug);
                        dia.Ejercicios.Add(new EjercicioPrescrito
                        {
                            Ejercicio = ejercicio,
                            Series = bloque.Series,
                            Repeticiones = bloque.Repeticiones
                        });
                    }
                }

                // Un día sin ejercicios no se agrega: pasa si el catálogo está vacío
                // o no tiene nada de ese grupo, y una rutina con días huecos confunde.
                if (dia.Ejercicios.Count > 0)
                    rutina.Dias.Add(dia);
            }

            return rutina;
        }

        private IEnumerable<Ejercicio> Elegir(BloqueEjercicios bloque, HashSet<string> yaUsados)
        {
            var candidatos = _catalogo.ObtenerPorGrupoMuscular(bloque.GrupoMuscular)
                .Where(e => !yaUsados.Contains(e.Slug))
                .ToList();

            if (candidatos.Count == 0) return Array.Empty<Ejercicio>();

            if (EquiposPreferidos.Count > 0)
            {
                var preferidos = candidatos
                    .Where(e => EquiposPreferidos.Contains(e.Equipo, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                // Si el filtro deja el bloque sin nada, es mejor un ejercicio con otro
                // equipo que un día incompleto.
                if (preferidos.Count > 0) candidatos = preferidos;
            }

            return candidatos
                .OrderBy(e => OrdenEstable(e.Slug))
                .ThenBy(e => e.Slug, StringComparer.Ordinal)
                .Take(bloque.Cantidad);
        }

        /// <summary>
        /// Orden pseudoaleatorio pero reproducible: depende del slug y de la semilla.
        /// No se usa string.GetHashCode() porque .NET lo aleatoriza por proceso, así que
        /// la rutina cambiaría en cada reinicio del servidor.
        /// </summary>
        private int OrdenEstable(string slug)
        {
            unchecked
            {
                int hash = 17 + _semillaRotacion * 31;
                foreach (char c in slug)
                    hash = hash * 31 + c;
                return hash & 0x7FFFFFFF;
            }
        }
    }
}
