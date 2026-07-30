using FitnessCoach.Domain.Catalogos;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Ports;
using Microsoft.Extensions.Caching.Memory;

namespace FitnessCoach.Infrastructure.Repositories
{
    /// <summary>
    /// Decorador de caché sobre el catálogo de ejercicios (Decorator: implementa el mismo
    /// puerto que envuelve, así que nada arriba se enteró del cambio).
    ///
    /// El catálogo son 1300+ filas de referencia que se pueblan por semilla al arrancar y
    /// nadie modifica en caliente, así que consultarlo en cada petición es trabajo repetido:
    /// armar una rutina dispara una consulta por bloque, y la rutina se regenera en varias
    /// pantallas (Rutina, Progreso, y al registrar un entrenamiento).
    ///
    /// Acá solo vive el manejo de la caché; los índices los arma <see cref="IndiceEjercicios"/>.
    /// </summary>
    public class RepositorioEjerciciosEnCache : IRepositorioEjercicios
    {
        private const string Clave = "catalogo-ejercicios";

        /// <summary>
        /// El catálogo solo cambia al re-sembrar (o tocando la base a mano), y eso implica
        /// reinicio. La vigencia es una red de seguridad, no un mecanismo de refresco.
        /// </summary>
        private static readonly TimeSpan Vigencia = TimeSpan.FromHours(12);

        private readonly IRepositorioEjercicios _origen;
        private readonly IMemoryCache _cache;

        public RepositorioEjerciciosEnCache(IRepositorioEjercicios origen, IMemoryCache cache)
        {
            _origen = origen;
            _cache = cache;
        }

        public IReadOnlyList<Ejercicio> ObtenerTodos() => Indice().Todos;

        public Ejercicio? ObtenerPorSlug(string slug) => Indice().PorSlug(slug);

        public IReadOnlyList<Ejercicio> ObtenerPorGrupoMuscular(string grupoMuscular) =>
            Indice().PorGrupoMuscular(grupoMuscular);

        private IndiceEjercicios Indice() =>
            _cache.GetOrCreate(Clave, entrada =>
            {
                entrada.AbsoluteExpirationRelativeToNow = Vigencia;
                return IndiceEjercicios.Armar(_origen.ObtenerTodos());
            })!;
    }
}
