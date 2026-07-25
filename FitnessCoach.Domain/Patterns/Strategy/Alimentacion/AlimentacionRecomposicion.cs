using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public class AlimentacionRecomposicion : EstrategiaAlimentacionBase
    {
        public AlimentacionRecomposicion(IRepositorioAlimentos catalogo, int semillaRotacion = 0)
            : base(catalogo, semillaRotacion) { }

        protected override string NombrePlan => "Plan de Recomposición Corporal";
        protected override string Objetivo => "Recomposición y Fuerza";

        protected override string Descripcion =>
            "Calorías de mantenimiento con proteína alta: el objetivo es ganar músculo y perder " +
            "grasa a la vez, así que el peso en la balanza puede moverse poco aunque el cuerpo cambie.";

        protected override IReadOnlyList<PlantillaComida> Estructura => new[]
        {
            new PlantillaComida
            {
                Nombre = "Desayuno", Momento = "desayuno", Hora = "07:30", ParteDelDia = 0.25,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato"), new RolAlimento("fruta") }
            },
            new PlantillaComida
            {
                Nombre = "Almuerzo", Momento = "principal", Hora = "13:00", ParteDelDia = 0.30,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato"), new RolAlimento("verdura", 2), new RolAlimento("grasa") }
            },
            new PlantillaComida
            {
                Nombre = "Merienda", Momento = "snack", Hora = "17:00", ParteDelDia = 0.18,
                Roles = new[] { new RolAlimento("lacteo"), new RolAlimento("fruta"), new RolAlimento("grasa") }
            },
            new PlantillaComida
            {
                Nombre = "Cena", Momento = "principal", Hora = "20:30", ParteDelDia = 0.27,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato"), new RolAlimento("verdura", 2) }
            }
        };

        protected override IReadOnlyList<string> Recomendaciones => new[]
        {
            "Sostener la proteína alta todos los días: es lo que dirige las calorías al músculo y no a la grasa",
            "Medir el progreso con fotos y medidas, no solo con la balanza",
            "Entrenar con cargas progresivas: sin el estímulo, las calorías no se convierten en músculo",
            "Mantener las verduras en las dos comidas principales",
            "Ser paciente: la recomposición es el proceso más lento de los tres"
        };
    }
}
