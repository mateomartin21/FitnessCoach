using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public class AlimentacionPerderPeso : IEstrategiaAlimentacion
    {
        public PlanAlimentacion GenerarPlan()
        {
            return new PlanAlimentacion
            {
                NombrePlan = "Plan Déficit Calórico",
                Objetivo = "Pérdida de Grasa",
                CaloriasObjetivo = "1800-2000 kcal/día",
                Descripcion = "Alto en proteína, bajo en carbohidratos simples. Déficit de 300-500 kcal respecto al mantenimiento.",
                Comidas = new List<ComidaDia>
                {
                    new ComidaDia { NombreComida = "Desayuno", Hora = "07:00", Calorias = 380, Proteinas = 30, Carbohidratos = 35, Grasas = 10,
                        Alimentos = new List<string> { "4 claras de huevo + 1 huevo entero revueltos", "70g avena en agua con canela", "1 manzana mediana" } },
                    new ComidaDia { NombreComida = "Snack Mañana", Hora = "10:00", Calorias = 180, Proteinas = 20, Carbohidratos = 15, Grasas = 5,
                        Alimentos = new List<string> { "150g yogur griego natural 0%", "1 puñado de arándanos", "10g proteína en polvo opcional" } },
                    new ComidaDia { NombreComida = "Almuerzo", Hora = "13:00", Calorias = 520, Proteinas = 45, Carbohidratos = 45, Grasas = 12,
                        Alimentos = new List<string> { "180g pechuga de pollo a la plancha", "120g arroz integral cocido", "Ensalada grande con limón y aceite de oliva", "1 taza de brócoli al vapor" } },
                    new ComidaDia { NombreComida = "Merienda", Hora = "16:30", Calorias = 200, Proteinas = 25, Carbohidratos = 10, Grasas = 6,
                        Alimentos = new List<string> { "2 tortitas de arroz", "100g atún en agua", "Pepino y zanahoria en bastones" } },
                    new ComidaDia { NombreComida = "Cena", Hora = "20:00", Calorias = 420, Proteinas = 40, Carbohidratos = 20, Grasas = 15,
                        Alimentos = new List<string> { "200g salmón o merluza al horno", "150g batata cocida", "Ensalada de espinacas con tomate", "1 cucharada aceite de oliva" } }
                },
                RecomendacionesGenerales = new List<string>
                {
                    "Consumir mínimo 2.5 litros de agua al día",
                    "No saltarse comidas para evitar catabolismo muscular",
                    "Priorizar proteína en cada comida para preservar masa muscular",
                    "Evitar azúcares simples y ultraprocesados",
                    "Cenar al menos 2 horas antes de dormir"
                }
            };
        }
    }
}