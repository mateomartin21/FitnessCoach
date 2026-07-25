namespace FitnessCoach.Domain.Models.Alimentacion
{
    /// <summary>
    /// Reparto de macronutrientes objetivo de un día, en gramos.
    /// Es lo que un profesional prescribe: no "1800 kcal" a secas, sino cuántos
    /// gramos de cada macro sostienen esas calorías para este objetivo.
    /// </summary>
    public readonly record struct ObjetivoMacros(
        int Calorias,
        int ProteinaG,
        int GrasaG,
        int CarbohidratoG)
    {
        /// <summary>Factores de Atwater: energía por gramo de cada macronutriente.</summary>
        public const int KcalPorGramoProteina = 4;
        public const int KcalPorGramoCarbohidrato = 4;
        public const int KcalPorGramoGrasa = 9;

        public int CaloriasDeProteina => ProteinaG * KcalPorGramoProteina;
        public int CaloriasDeGrasa => GrasaG * KcalPorGramoGrasa;
        public int CaloriasDeCarbohidrato => CarbohidratoG * KcalPorGramoCarbohidrato;

        /// <summary>Suma real de los macros. Puede diferir del total por redondeo a gramos enteros.</summary>
        public int CaloriasSegunMacros => CaloriasDeProteina + CaloriasDeGrasa + CaloriasDeCarbohidrato;

        public int PorcentajeProteina => Porcentaje(CaloriasDeProteina);
        public int PorcentajeGrasa => Porcentaje(CaloriasDeGrasa);
        public int PorcentajeCarbohidrato => Porcentaje(CaloriasDeCarbohidrato);

        private int Porcentaje(int caloriasDelMacro) =>
            Calorias <= 0 ? 0 : (int)Math.Round(caloriasDelMacro * 100.0 / Calorias);
    }
}
