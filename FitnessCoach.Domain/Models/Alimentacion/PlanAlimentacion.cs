namespace FitnessCoach.Domain.Models.Alimentacion
{
    public class PlanAlimentacion
    {
        public string NombrePlan { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty;
        public string CaloriasObjetivo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public List<ComidaDia> Comidas { get; set; } = new();
        public List<string> RecomendacionesGenerales { get; set; } = new();
    }
}
