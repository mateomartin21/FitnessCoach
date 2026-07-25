using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public class AlimentacionPerderPeso : EstrategiaAlimentacionBase
    {
        public AlimentacionPerderPeso(IRepositorioAlimentos catalogo, int semillaRotacion = 0)
            : base(catalogo, semillaRotacion) { }

        protected override string NombrePlan => "Plan Déficit Calórico";
        protected override string Objetivo => "Pérdida de Grasa";

        protected override string Descripcion =>
            "Alto en proteína para preservar masa muscular durante el déficit, con verduras " +
            "en todas las comidas principales: aportan volumen y saciedad por muy pocas calorías.";

        // Cinco comidas para que ninguna quede tan chica que dé hambre a la hora.
        // El almuerzo y la cena se llevan la mayor parte; los snacks sostienen el medio.
        protected override IReadOnlyList<PlantillaComida> Estructura => new[]
        {
            new PlantillaComida
            {
                Nombre = "Desayuno", Momento = "desayuno", Hora = "07:00", ParteDelDia = 0.22,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato"), new RolAlimento("fruta") }
            },
            new PlantillaComida
            {
                Nombre = "Snack de media mañana", Momento = "snack", Hora = "10:00", ParteDelDia = 0.12,
                Roles = new[] { new RolAlimento("lacteo"), new RolAlimento("fruta") }
            },
            new PlantillaComida
            {
                Nombre = "Almuerzo", Momento = "principal", Hora = "13:00", ParteDelDia = 0.30,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato"), new RolAlimento("verdura", 2) }
            },
            new PlantillaComida
            {
                Nombre = "Merienda", Momento = "snack", Hora = "16:30", ParteDelDia = 0.12,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("verdura") }
            },
            new PlantillaComida
            {
                Nombre = "Cena", Momento = "principal", Hora = "20:00", ParteDelDia = 0.24,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("verdura", 2), new RolAlimento("grasa") }
            }
        };

        protected override IReadOnlyList<string> Recomendaciones => new[]
        {
            "Priorizar proteína en cada comida: es lo que protege el músculo mientras se pierde grasa",
            "Llenar medio plato con verduras en el almuerzo y la cena",
            "No saltarse comidas: llegar con hambre extrema a la siguiente lleva a comer de más",
            "Cocinar a la plancha, al horno o al vapor antes que fritos",
            "Cenar al menos dos horas antes de dormir"
        };
    }
}
