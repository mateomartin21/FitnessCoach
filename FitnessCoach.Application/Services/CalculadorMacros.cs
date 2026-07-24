using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Application.Services
{
    /// <summary>
    /// Reparte las calorías diarias en gramos de proteína, grasa y carbohidrato.
    ///
    /// Sigue el orden con que trabaja un profesional, que no es arbitrario:
    ///   1. La proteína se fija por peso corporal — es el macro con requerimiento
    ///      absoluto, no proporcional a las calorías.
    ///   2. La grasa se fija como porcentaje del total, con un piso por debajo del
    ///      cual se compromete la función hormonal.
    ///   3. Los carbohidratos absorben el resto: son la fuente de energía flexible.
    ///
    /// Es una función pura sobre números: sin estado, sin base de datos, testeable.
    /// </summary>
    public static class CalculadorMacros
    {
        /// <summary>Piso de seguridad: por debajo del 15% de las calorías, la grasa compromete la salud hormonal.</summary>
        private const double PorcentajeGrasaMinimo = 0.15;

        /// <summary>
        /// Si tras cubrir proteína y grasa no quedan calorías para carbohidratos,
        /// se recorta la proteína hasta este piso antes que dejar el plan en cero.
        /// </summary>
        private const double ProteinaMinimaPorKg = 1.2;

        public static ObjetivoMacros Calcular(UsuarioPerfil usuario, double caloriasDiarias)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            if (caloriasDiarias <= 0)
                throw new ArgumentOutOfRangeException(nameof(caloriasDiarias), caloriasDiarias,
                    "No se pueden repartir macros sobre calorías nulas o negativas.");

            var objetivo = usuario.ObjetivoActual;

            // Sin objetivo definido se usa un reparto equilibrado de referencia.
            double proteinaPorKg = objetivo?.GramosProteinaPorKg ?? 1.6;
            double porcentajeGrasa = Math.Max(
                objetivo?.PorcentajeCaloriasDeGrasa ?? 0.25,
                PorcentajeGrasaMinimo);

            double proteinaG = usuario.PesoKg * proteinaPorKg;
            double grasaG = caloriasDiarias * porcentajeGrasa / ObjetivoMacros.KcalPorGramoGrasa;

            double caloriasRestantes = caloriasDiarias
                - proteinaG * ObjetivoMacros.KcalPorGramoProteina
                - grasaG * ObjetivoMacros.KcalPorGramoGrasa;

            // Puede pasar con una persona muy pesada y pocas calorías: proteína y grasa
            // solas ya cubren el total. Se recorta la proteína en vez de devolver
            // carbohidratos negativos, que sería un plan imposible de cumplir.
            if (caloriasRestantes < 0)
            {
                double proteinaMinima = usuario.PesoKg * ProteinaMinimaPorKg;
                double caloriasParaProteina = caloriasDiarias - grasaG * ObjetivoMacros.KcalPorGramoGrasa;

                proteinaG = Math.Max(
                    proteinaMinima,
                    Math.Max(0, caloriasParaProteina / ObjetivoMacros.KcalPorGramoProteina));

                caloriasRestantes = caloriasDiarias
                    - proteinaG * ObjetivoMacros.KcalPorGramoProteina
                    - grasaG * ObjetivoMacros.KcalPorGramoGrasa;
            }

            double carbohidratoG = Math.Max(0, caloriasRestantes / ObjetivoMacros.KcalPorGramoCarbohidrato);

            return new ObjetivoMacros(
                Calorias: (int)Math.Round(caloriasDiarias),
                ProteinaG: (int)Math.Round(proteinaG),
                GrasaG: (int)Math.Round(grasaG),
                CarbohidratoG: (int)Math.Round(carbohidratoG));
        }
    }
}
