using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Tests.Fakes
{
    /// <summary>
    /// Catálogo de mentira, en memoria. Permite probar la composición de rutinas sin
    /// base de datos y con un contenido conocido (ADR-08).
    /// </summary>
    public class RepositorioEjerciciosFalso : IRepositorioEjercicios
    {
        private readonly List<Ejercicio> _ejercicios = new();

        public RepositorioEjerciciosFalso(params Ejercicio[] ejercicios)
        {
            int siguienteId = 1;
            foreach (var ejercicio in ejercicios)
            {
                if (ejercicio.Id == 0) ejercicio.Id = siguienteId++;
                _ejercicios.Add(ejercicio);
            }
        }

        /// <summary>
        /// Cataloga <paramref name="cantidad"/> ejercicios por grupo, con nombres predecibles.
        /// </summary>
        public static RepositorioEjerciciosFalso ConGrupos(int cantidad, params string[] grupos)
        {
            var ejercicios = new List<Ejercicio>();
            foreach (var grupo in grupos)
            {
                for (int i = 1; i <= cantidad; i++)
                {
                    ejercicios.Add(new Ejercicio
                    {
                        Slug = $"{grupo}-ejercicio-{i}",
                        Nombre = $"{grupo} {i}",
                        GrupoMuscular = grupo,
                        Equipo = i % 2 == 0 ? "dumbbell" : "barbell",
                        UrlGif = $"https://cdn.example/{grupo}-{i}.gif"
                    });
                }
            }
            return new RepositorioEjerciciosFalso(ejercicios.ToArray());
        }

        public IReadOnlyList<Ejercicio> ObtenerTodos() => _ejercicios;

        public Ejercicio? ObtenerPorSlug(string slug) =>
            _ejercicios.FirstOrDefault(e => e.Slug == slug);

        public IReadOnlyList<Ejercicio> ObtenerPorGrupoMuscular(string grupoMuscular) =>
            _ejercicios.Where(e => e.GrupoMuscular == grupoMuscular).ToList();
    }
}
