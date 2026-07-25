using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Patterns.Decorator;
using FitnessCoach.Domain.Patterns.Strategy.Alimentacion;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Arma el plan de comidas del usuario. Es el punto donde se encadena todo:
    /// perfil → calorías diarias → reparto en macros → comidas concretas del catálogo.
    /// </summary>
    public class GeneradorAlimentacionService : IGeneradorAlimentacion
    {
        private readonly IRepositorioAlimentos _catalogo;
        private readonly ICalculadorCalorico _calculadorCalorico;

        public GeneradorAlimentacionService(
            IRepositorioAlimentos catalogo,
            ICalculadorCalorico calculadorCalorico)
        {
            _catalogo = catalogo ?? throw new ArgumentNullException(nameof(catalogo));
            _calculadorCalorico = calculadorCalorico ?? throw new ArgumentNullException(nameof(calculadorCalorico));
        }

        public PlanAlimentacion GenerarPlanPara(UsuarioPerfil usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var calorias = _calculadorCalorico.CalcularCaloriasDiarias(usuario);
            var macros = CalculadorMacros.Calcular(usuario, calorias);

            var semilla = SemillaDe(usuario);
            var preferencias = usuario.Preferencias;

            IEstrategiaAlimentacion estrategia = usuario.ObjetivoActual switch
            {
                ObjetivoPerderPeso => new AlimentacionPerderPeso(_catalogo, semilla, preferencias),
                ObjetivoGanarMusculo => new AlimentacionGanarMusculo(_catalogo, semilla, preferencias),
                ObjetivoRecomposicion => new AlimentacionRecomposicion(_catalogo, semilla, preferencias),
                _ => new AlimentacionRecomposicion(_catalogo, semilla, preferencias)
            };

            return new PlanConHidratacion(estrategia).GenerarPlan(macros);
        }

        /// <summary>
        /// Semilla de rotación por usuario: dos personas con el mismo objetivo ven
        /// alimentos distintos, pero cada una ve siempre el mismo plan.
        /// </summary>
        private static int SemillaDe(UsuarioPerfil usuario) => usuario.Id;
    }
}
