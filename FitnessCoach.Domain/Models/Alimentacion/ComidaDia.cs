namespace FitnessCoach.Domain.Models.Alimentacion
{
    public class ComidaDia
    {
        public string NombreComida { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public List<string> Alimentos { get; set; } = new();
        public int Calorias { get; set; }
        public int Proteinas { get; set; }
        public int Carbohidratos { get; set; }
        public int Grasas { get; set; }
    }
}
