using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public class AlimentacionGanarMusculo : EstrategiaAlimentacionBase
    {
        public AlimentacionGanarMusculo(IRepositorioAlimentos catalogo, int semillaRotacion = 0,
            PreferenciasAlimentarias? preferencias = null)
            : base(catalogo, semillaRotacion, preferencias) { }

        protected override string NombrePlan => "Plan Superávit Calórico";
        protected override string Objetivo => "Ganancia Muscular";

        protected override string Descripcion =>
            "Seis comidas para repartir la carga: con un superávit, meter todas las calorías " +
            "en tres platos se vuelve incómodo. Los carbohidratos se concentran alrededor del " +
            "entrenamiento, que es cuando el músculo mejor los aprovecha.";

        // Seis comidas, con dos alrededor del entrenamiento. El pre y el post son
        // chicos a propósito: su función es el momento, no el volumen.
        protected override IReadOnlyList<PlantillaComida> Estructura => new[]
        {
            new PlantillaComida
            {
                Nombre = "Desayuno", Momento = "desayuno", Hora = "07:00", ParteDelDia = 0.22,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato"), new RolAlimento("fruta"), new RolAlimento("grasa") }
            },
            new PlantillaComida
            {
                Nombre = "Snack de media mañana", Momento = "snack", Hora = "10:00", ParteDelDia = 0.12,
                Roles = new[] { new RolAlimento("lacteo"), new RolAlimento("carbohidrato") }
            },
            new PlantillaComida
            {
                Nombre = "Almuerzo", Momento = "principal", Hora = "13:00", ParteDelDia = 0.26,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato"), new RolAlimento("verdura"), new RolAlimento("grasa") }
            },
            new PlantillaComida
            {
                Nombre = "Pre-entreno", Momento = "snack", Hora = "16:00", ParteDelDia = 0.10,
                Roles = new[] { new RolAlimento("carbohidrato"), new RolAlimento("fruta") }
            },
            new PlantillaComida
            {
                Nombre = "Post-entreno", Momento = "snack", Hora = "18:30", ParteDelDia = 0.10,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato") }
            },
            new PlantillaComida
            {
                Nombre = "Cena", Momento = "principal", Hora = "21:00", ParteDelDia = 0.20,
                Roles = new[] { new RolAlimento("proteina"), new RolAlimento("carbohidrato"), new RolAlimento("verdura") }
            }
        };

        protected override IReadOnlyList<string> Recomendaciones => new[]
        {
            "Repartir la proteína en todas las comidas: el cuerpo no aprovecha de golpe la del día entero",
            "Concentrar los carbohidratos alrededor del entrenamiento",
            "No recortar las grasas: hacen falta para la producción hormonal",
            "Si cuesta llegar a las calorías, sumar frutos secos y aceite antes que ultraprocesados",
            "Dormir al menos ocho horas: el músculo se construye descansando, no entrenando"
        };
    }
}
