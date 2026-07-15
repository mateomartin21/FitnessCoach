using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public class AlimentacionPerderPeso : IEstrategiaAlimentacion
    {
        public PlanAlimentacion GenerarPlan()
        {
            return new PlanAlimentacion
            {
                NombrePlan = "Plan Deficit Calorico",
                Objetivo = "Perdida de Grasa",
                CaloriasObjetivo = "1800-2000 kcal/dia",
                Descripcion = "Alto en proteina, bajo en carbohidratos simples. Deficit de 300-500 kcal respecto al mantenimiento.",
                Comidas = new List<ComidaDia>
                {
                    new ComidaDia { NombreComida = "Desayuno", Hora = "07:00", Calorias = 380, Proteinas = 30, Carbohidratos = 35, Grasas = 10,
                        Alimentos = new List<string> { "4 claras de huevo + 1 huevo entero revueltos", "70g avena en agua con canela", "1 manzana mediana" } },
                    new ComidaDia { NombreComida = "Snack Manana", Hora = "10:00", Calorias = 180, Proteinas = 20, Carbohidratos = 15, Grasas = 5,
                        Alimentos = new List<string> { "150g yogur griego natural 0%", "1 puñado de arandanos", "10g proteina en polvo opcional" } },
                    new ComidaDia { NombreComida = "Almuerzo", Hora = "13:00", Calorias = 520, Proteinas = 45, Carbohidratos = 45, Grasas = 12,
                        Alimentos = new List<string> { "180g pechuga de pollo a la plancha", "120g arroz integral cocido", "Ensalada grande con limon y aceite de oliva", "1 taza de brocoli al vapor" } },
                    new ComidaDia { NombreComida = "Merienda", Hora = "16:30", Calorias = 200, Proteinas = 25, Carbohidratos = 10, Grasas = 6,
                        Alimentos = new List<string> { "2 tortitas de arroz", "100g atun en agua", "Pepino y zanahoria en bastones" } },
                    new ComidaDia { NombreComida = "Cena", Hora = "20:00", Calorias = 420, Proteinas = 40, Carbohidratos = 20, Grasas = 15,
                        Alimentos = new List<string> { "200g salmon o merluza al horno", "150g batata cocida", "Ensalada de espinacas con tomate", "1 cucharada aceite de oliva" } }
                },
                RecomendacionesGenerales = new List<string>
                {
                    "Consumir minimo 2.5 litros de agua al dia",
                    "No saltarse comidas para evitar catabolismo muscular",
                    "Priorizar proteina en cada comida para preservar masa muscular",
                    "Evitar azucares simples y ultraprocesados",
                    "Cenar al menos 2 horas antes de dormir"
                }
            };
        }
    }
}
