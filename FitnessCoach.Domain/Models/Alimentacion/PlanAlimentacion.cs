namespace FitnessCoach.Domain.Models.Alimentacion
{
    public class PlanAlimentacion
    {
        public string NombrePlan { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Los macros que el plan intenta cubrir, calculados desde el peso y el objetivo
        /// del usuario. Antes era el texto fijo "1800-2000 kcal/día", igual para todos.
        /// </summary>
        public ObjetivoMacros Objetivos { get; set; }

        public List<ComidaDia> Comidas { get; set; } = new();
        public List<string> RecomendacionesGenerales { get; set; } = new();

        // Lo que el plan realmente aporta, sumando las comidas. Puede no coincidir
        // exactamente con los objetivos: los alimentos vienen en porciones razonables,
        // no en las cantidades que cuadrarían la cuenta al gramo.
        public int CaloriasTotales => Comidas.Sum(c => c.Calorias);
        public int ProteinaTotalG => Comidas.Sum(c => c.Proteinas);
        public int CarbohidratoTotalG => Comidas.Sum(c => c.Carbohidratos);
        public int GrasaTotalG => Comidas.Sum(c => c.Grasas);

        /// <summary>Desvío entre lo que aporta el plan y el objetivo, en porcentaje.</summary>
        public double DesvioCaloricoPorcentaje =>
            Objetivos.Calorias == 0 ? 0
            : (CaloriasTotales - Objetivos.Calorias) * 100.0 / Objetivos.Calorias;

        /// <summary>
        /// Advertencia obligatoria. Un plan calculado desde fórmulas es un punto de
        /// partida orientativo, no una indicación médica: las fórmulas no saben de
        /// patologías, medicación, embarazo ni alergias.
        /// </summary>
        public const string Descargo =
            "Este plan es orientativo y se genera a partir de fórmulas generales. " +
            "No reemplaza la consulta con un nutricionista o médico, especialmente si " +
            "tienes alguna condición de salud, tomas medicación, estás embarazada o " +
            "tienes alergias alimentarias.";
    }
}
