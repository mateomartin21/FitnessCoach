namespace FitnessCoach.Domain.Models.Alimentacion
{
    /// <summary>
    /// Lo comido en un día frente al objetivo de macros. Cálculo puro: recibe los
    /// registros y el objetivo, no toca base de datos ni catálogo.
    /// </summary>
    public sealed class ResumenDiario
    {
        public DateOnly Dia { get; }
        public ObjetivoMacros Objetivo { get; }
        public IReadOnlyList<RegistroComida> Registros { get; }

        public ResumenDiario(DateOnly dia, ObjetivoMacros objetivo, IReadOnlyList<RegistroComida> registros)
        {
            Dia = dia;
            Objetivo = objetivo;
            Registros = registros ?? Array.Empty<RegistroComida>();
        }

        public int CaloriasConsumidas => (int)Math.Round(Registros.Sum(r => r.Calorias));
        public int ProteinaConsumidaG => (int)Math.Round(Registros.Sum(r => r.ProteinaG));
        public int CarbohidratoConsumidoG => (int)Math.Round(Registros.Sum(r => r.CarbohidratoG));
        public int GrasaConsumidaG => (int)Math.Round(Registros.Sum(r => r.GrasaG));

        // Lo que falta para llegar al objetivo; nunca baja de cero, pasarse no "resta".
        public int CaloriasRestantes => Math.Max(0, Objetivo.Calorias - CaloriasConsumidas);
        public int ProteinaRestanteG => Math.Max(0, Objetivo.ProteinaG - ProteinaConsumidaG);

        /// <summary>Qué porción del objetivo calórico se cubrió (0 a 100+, puede pasarse).</summary>
        public int PorcentajeCalorico => Objetivo.Calorias == 0
            ? 0
            : (int)Math.Round(CaloriasConsumidas * 100.0 / Objetivo.Calorias);

        /// <summary>Se pasó del objetivo calórico (con un margen del 5% que no se considera pasarse).</summary>
        public bool SePaso => CaloriasConsumidas > Objetivo.Calorias * 1.05;

        public bool SinRegistros => Registros.Count == 0;
    }
}
