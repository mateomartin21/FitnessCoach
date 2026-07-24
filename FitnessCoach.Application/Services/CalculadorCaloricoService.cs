using FitnessCoach.Domain.Models;

namespace FitnessCoach.Application.Services
{
    public class CalculadorCaloricoService : ICalculadorCalorico
    {
        public double CalcularCaloriasDiarias(UsuarioPerfil usuario)
        {
            // Las anotaciones del modelo solo actúan si alguien las evalúa (el pipeline de MVC).
            // Acá el cálculo se defiende solo: con estatura 0 la fórmula devuelve un número
            // perfectamente formado y perfectamente falso, que es el peor tipo de bug.
            ArgumentNullException.ThrowIfNull(usuario);
            ValidarRango(usuario.PesoKg, RangosPerfil.PesoMinimoKg, RangosPerfil.PesoMaximoKg,
                nameof(usuario.PesoKg), "kg");
            ValidarRango(usuario.EstaturaCm, RangosPerfil.EstaturaMinimaCm, RangosPerfil.EstaturaMaximaCm,
                nameof(usuario.EstaturaCm), "cm");
            ValidarRango(usuario.Edad, RangosPerfil.EdadMinima, RangosPerfil.EdadMaxima,
                nameof(usuario.Edad), "años");

            // 1. Cálculo del Metabolismo Basal (Fórmula de Mifflin-St Jeor simplificada)
            double metabolismoBasal = (10 * usuario.PesoKg) + (6.25 * usuario.EstaturaCm) - (5 * usuario.Edad) + 5;

            // 2. Multiplicador de actividad (factor de actividad moderada base de 1.375)
            double caloriasMantenimiento = metabolismoBasal * 1.375;

            // 3. Aplicar el multiplicador del objetivo
            if (usuario.ObjetivoActual != null)
            {
                return caloriasMantenimiento * usuario.ObjetivoActual.CalcularMultiplicadorCalorico();
            }

            return caloriasMantenimiento;
        }

        private static void ValidarRango(double valor, double minimo, double maximo, string campo, string unidad)
        {
            if (valor < minimo || valor > maximo)
                throw new ArgumentOutOfRangeException(
                    campo, valor,
                    $"No se puede calcular las calorías: {campo} debe estar entre {minimo} y {maximo} {unidad}.");
        }
    }
}
