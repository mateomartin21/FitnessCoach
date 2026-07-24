namespace FitnessCoach.Domain.Models.Alimentacion
{
    /// <summary>
    /// Una comida del plan. Sus macros ya no se escriben a mano: se suman desde las
    /// porciones, así que no pueden contradecir a los alimentos que la comida lista.
    /// </summary>
    public class ComidaDia
    {
        public string NombreComida { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;

        public List<PorcionAlimento> Porciones { get; set; } = new();

        /// <summary>Los alimentos tal como se muestran, para las vistas.</summary>
        public IEnumerable<string> Alimentos => Porciones.Select(p => p.Descripcion);

        public int Calorias => (int)Math.Round(Porciones.Sum(p => p.Macros.Calorias));
        public int Proteinas => (int)Math.Round(Porciones.Sum(p => p.Macros.ProteinaG));
        public int Carbohidratos => (int)Math.Round(Porciones.Sum(p => p.Macros.CarbohidratoG));
        public int Grasas => (int)Math.Round(Porciones.Sum(p => p.Macros.GrasaG));
    }
}
