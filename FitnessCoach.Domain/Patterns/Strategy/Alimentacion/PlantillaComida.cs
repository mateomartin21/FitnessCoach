namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    /// <summary>
    /// Un puesto a cubrir dentro de una comida: "acá va una proteína", "acá una verdura".
    /// Describe el papel nutricional, no el alimento: el concreto lo elige el catálogo.
    /// </summary>
    /// <param name="Categoria">Categoría del catálogo ("proteina", "carbohidrato", "verdura", "fruta", "grasa", "lacteo").</param>
    /// <param name="Cantidad">Cuántos alimentos distintos de ese papel.</param>
    public readonly record struct RolAlimento(string Categoria, int Cantidad = 1);

    /// <summary>Plantilla de una comida, sin alimentos concretos todavía.</summary>
    public sealed class PlantillaComida
    {
        public string Nombre { get; init; } = string.Empty;
        public string Hora { get; init; } = string.Empty;

        /// <summary>
        /// Qué parte del total diario aporta esta comida (0 a 1). La suma de todas las
        /// comidas del plan debería dar 1.
        /// </summary>
        public double ParteDelDia { get; init; }

        /// <summary>
        /// Momento del día al que pertenece la comida: "desayuno", "principal" o "snack".
        /// Filtra el catálogo para que el desayuno no traiga tempeh con pasta.
        /// </summary>
        public string Momento { get; init; } = "principal";

        public IReadOnlyList<RolAlimento> Roles { get; init; } = Array.Empty<RolAlimento>();
    }
}
