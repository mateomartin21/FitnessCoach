using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public class CalculadorCaloricoService : ICalculadorCalorico
    {
        public double CalcularCaloriasDiarias(UsuarioPerfil usuario)
        {
            // 1. Cálculo del Metabolismo Basal (Fórmula de Mifflin-St Jeor simplificada)
            double metabolismoBasal = (10 * usuario.PesoKg) + (6.25 * usuario.EstaturaCm) - (5 * usuario.Edad) + 5;

            // 2. Multiplicador de actividad (Asumiremos un factor de actividad moderada base de 1.375)
            double caloriasMantenimiento = metabolismoBasal * 1.375;

            // 3. Aplicar el multiplicador del objetivo (OCP aplicado en los modelos)
            if (usuario.ObjetivoActual != null)
            {
                return caloriasMantenimiento * usuario.ObjetivoActual.CalcularMultiplicadorCalorico();
            }

            return caloriasMantenimiento;
        }
    }
}
