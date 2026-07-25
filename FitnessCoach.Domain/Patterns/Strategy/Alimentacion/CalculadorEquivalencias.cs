using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    /// <summary>
    /// Calcula sustituciones equivalentes para una porción del plan.
    ///
    /// El criterio es el de la tabla de intercambios que usa un nutricionista: dos
    /// alimentos del mismo grupo se pueden cambiar entre sí si aportan lo mismo del
    /// macro que los define. Se iguala ese macro (la proteína del pollo, el carbohidrato
    /// del arroz) ajustando los gramos del sustituto, y se descartan los que exigirían
    /// una porción fuera de lo razonable.
    ///
    /// Es una función pura sobre datos del catálogo: sin estado, sin base de datos.
    /// </summary>
    public static class CalculadorEquivalencias
    {
        /// <summary>
        /// Cuánto puede alejarse la porción equivalente de los límites del alimento antes
        /// de descartarla. Sustituir 150 g de pollo por 400 g de brócoli iguala la
        /// proteína en el papel, pero nadie come ese plato.
        /// </summary>
        private const double MultiploDeRedondeoG = 5;

        /// <summary>
        /// Sustitutos para <paramref name="original"/>, tomados de <paramref name="candidatos"/>
        /// (que se esperan del mismo grupo de intercambio), ordenados del más parecido en
        /// calorías al menos, y a lo sumo <paramref name="cuantos"/>.
        /// </summary>
        public static IReadOnlyList<SustitucionAlimento> Para(
            PorcionAlimento original,
            IEnumerable<Alimento> candidatos,
            int cuantos = 3)
        {
            ArgumentNullException.ThrowIfNull(original);
            ArgumentNullException.ThrowIfNull(candidatos);

            var objetivoMacro = original.Alimento.GramosDelMacroPrincipal(original.Gramos);
            var caloriasOriginal = original.Macros.Calorias;

            var sustitutos = new List<SustitucionAlimento>();

            foreach (var candidato in candidatos)
            {
                // El propio alimento no es una alternativa a sí mismo.
                if (candidato.Slug == original.Alimento.Slug) continue;

                // El candidato tiene que aportar de verdad el macro que se iguala; si no,
                // la regla de tres no tiene sentido (no se sustituye pollo por lechuga).
                var densidad = DensidadDelMacro(candidato, original.Alimento.MacroPrincipal);
                if (densidad <= 0) continue;

                var gramos = Redondear(objetivoMacro / densidad);

                // Fuera de la porción razonable del alimento, la equivalencia es de
                // papel: técnicamente iguala el macro, pero no es un plato que alguien coma.
                if (gramos < candidato.PorcionMinimaG || gramos > candidato.PorcionMaximaG)
                    continue;

                sustitutos.Add(new SustitucionAlimento { Alimento = candidato, Gramos = gramos });
            }

            return sustitutos
                .OrderBy(s => Math.Abs(s.Macros.Calorias - caloriasOriginal))
                .ThenBy(s => s.Alimento.Slug, StringComparer.Ordinal)
                .Take(cuantos)
                .ToList();
        }

        private static double DensidadDelMacro(Alimento alimento, string macro) => macro switch
        {
            "proteina" => alimento.ProteinaPor100g / 100.0,
            "carbohidrato" => alimento.CarbohidratoPor100g / 100.0,
            _ => alimento.GrasaPor100g / 100.0
        };

        private static double Redondear(double gramos) =>
            Math.Max(MultiploDeRedondeoG, Math.Round(gramos / MultiploDeRedondeoG) * MultiploDeRedondeoG);
    }
}
