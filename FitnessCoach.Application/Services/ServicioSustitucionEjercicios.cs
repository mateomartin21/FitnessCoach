using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Services
{
    public class ServicioSustitucionEjercicios : IServicioSustitucionEjercicios
    {
        private readonly IRepositorioEjercicios _catalogo;

        public ServicioSustitucionEjercicios(IRepositorioEjercicios catalogo)
        {
            _catalogo = catalogo ?? throw new ArgumentNullException(nameof(catalogo));
        }

        public Ejercicio? EnUso(UsuarioPerfil usuario, string slugReferencia)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            return _catalogo.ObtenerPorSlug(SlugEnUso(usuario, slugReferencia));
        }

        public IReadOnlyList<Ejercicio> Alternativas(UsuarioPerfil usuario, string slugReferencia, string? busqueda = null)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var original = _catalogo.ObtenerPorSlug(slugReferencia);
            if (original is null) return Array.Empty<Ejercicio>();

            var enUso = SlugEnUso(usuario, slugReferencia);
            var preferencias = usuario.PreferenciasEntrenamiento;

            var mismoGrupo = _catalogo.ObtenerPorGrupoMuscular(original.GrupoMuscular)
                .Where(e => !string.Equals(e.Slug, enUso, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Si el equipo del usuario deja la lista vacía se ofrecen todas: peor que una
            // alternativa con otro equipo es no poder cambiar el ejercicio.
            var alcanzables = mismoGrupo.Where(preferencias.Permite).ToList();
            if (alcanzables.Count > 0) mismoGrupo = alcanzables;

            if (!string.IsNullOrWhiteSpace(busqueda))
                mismoGrupo = mismoGrupo
                    .Where(e => e.Nombre.Contains(busqueda.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

            return mismoGrupo
                .OrderBy(e => e.Nombre, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        public bool Sustituir(UsuarioPerfil usuario, string slugReferencia, string slugElegido)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var original = _catalogo.ObtenerPorSlug(slugReferencia);
            var elegido = _catalogo.ObtenerPorSlug(slugElegido);

            if (original is null || elegido is null) return false;

            if (!string.Equals(original.GrupoMuscular, elegido.GrupoMuscular, StringComparison.OrdinalIgnoreCase))
                return false;

            if (string.Equals(original.Slug, elegido.Slug, StringComparison.OrdinalIgnoreCase))
            {
                // Volver al original es deshacer, no guardar un cambio a sí mismo.
                Restaurar(usuario, slugReferencia);
                return true;
            }

            usuario.PreferenciasEntrenamiento.Sustituciones[original.Slug] = elegido.Slug;
            return true;
        }

        public void Restaurar(UsuarioPerfil usuario, string slugReferencia)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            usuario.PreferenciasEntrenamiento.Sustituciones.Remove(slugReferencia);
        }

        private static string SlugEnUso(UsuarioPerfil usuario, string slugReferencia) =>
            usuario.PreferenciasEntrenamiento.Sustituciones.TryGetValue(slugReferencia, out var elegido)
                ? elegido
                : slugReferencia;
    }
}
