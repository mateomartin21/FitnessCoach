using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Patterns.Strategy;
using FitnessCoach.Domain.Patterns.Decorator;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Services
{
    public class GeneradorRutinasService : IGeneradorRutinas
    {
        private readonly IRepositorioEjercicios _catalogo;

        public GeneradorRutinasService(IRepositorioEjercicios catalogo)
        {
            _catalogo = catalogo;
        }

        public Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo, int semillaRotacion = 0,
                                               PreferenciasEntrenamiento? preferencias = null)
        {
            // PATRÓN STRATEGY — selecciona la estrategia según el objetivo.
            // Cada una recibe el catálogo: ya no trae los ejercicios adentro.
            IEstrategiaRutina estrategia = objetivo switch
            {
                ObjetivoPerderPeso => new EstrategiaPerderPeso(_catalogo, semillaRotacion, preferencias),
                ObjetivoGanarMusculo => new EstrategiaGanarMusculo(_catalogo, semillaRotacion, preferencias),
                ObjetivoRecomposicion => new EstrategiaRecomposicion(_catalogo, semillaRotacion, preferencias),
                _ => new EstrategiaRecomposicion(_catalogo, semillaRotacion, preferencias)
            };

            // PATRÓN DECORATOR — envuelve la estrategia con calentamiento y enfriamiento
            IEstrategiaRutina rutinaDecorada = new RutinaConEnfriamiento(
                                               new RutinaConCalentamiento(estrategia));

            var rutina = rutinaDecorada.GenerarRutina();

            AplicarSustituciones(rutina, preferencias);

            return rutina;
        }

        /// <summary>
        /// Cambia los ejercicios que el usuario pidió cambiar. Va al final y no dentro de la
        /// estrategia porque es una decisión del usuario, no del objetivo: la estrategia sigue
        /// eligiendo lo mismo y encima se aplica el cambio, así que deshacerlo devuelve la
        /// prescripción original sin recalcular nada.
        /// </summary>
        private void AplicarSustituciones(Rutina rutina, PreferenciasEntrenamiento? preferencias)
        {
            if (preferencias is null || preferencias.Sustituciones.Count == 0) return;

            foreach (var prescrito in rutina.Dias.SelectMany(d => d.Ejercicios))
            {
                if (!preferencias.Sustituciones.TryGetValue(prescrito.Ejercicio.Slug, out var slugElegido))
                    continue;

                var reemplazo = _catalogo.ObtenerPorSlug(slugElegido);

                // Un slug que ya no está en el catálogo se ignora y queda el original:
                // es mejor que dejar la fila vacía o tumbar la pantalla.
                if (reemplazo is null) continue;

                prescrito.SlugOriginal = prescrito.Ejercicio.Slug;
                prescrito.Ejercicio = reemplazo;
            }
        }
    }
}
