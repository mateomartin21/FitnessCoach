using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public class AlimentacionRecomposicion : IEstrategiaAlimentacion
    {
        public PlanAlimentacion GenerarPlan()
        {
            return new PlanAlimentacion
            {
                NombrePlan = "Plan Mantenimiento Inteligente",
                Objetivo = "Recomposicion Corporal",
                CaloriasObjetivo = "2200-2500 kcal/dia",
                Descripcion = "Calorias de mantenimiento con alta proteina. Ciclo de carbohidratos segun dias de entrenamiento.",
                Comidas = new List<ComidaDia>
                {
                    new ComidaDia { NombreComida = "Desayuno", Hora = "07:00", Calorias = 480, Proteinas = 35, Carbohidratos = 50, Grasas = 12,
                        Alimentos = new List<string> { "3 huevos enteros revueltos", "80g avena con agua y canela", "1 fruta mediana", "Cafe negro" } },
                    new ComidaDia { NombreComida = "Snack Manana", Hora = "10:30", Calorias = 220, Proteinas = 22, Carbohidratos = 20, Grasas = 6,
                        Alimentos = new List<string> { "150g yogur griego", "1 puñado de nueces (20g)", "1 fruta pequeña" } },
                    new ComidaDia { NombreComida = "Almuerzo", Hora = "13:00", Calorias = 600, Proteinas = 48, Carbohidratos = 55, Grasas = 14,
                        Alimentos = new List<string> { "200g pechuga de pollo o atun", "130g arroz integral", "Ensalada mixta grande", "1/4 aguacate" } },
                    new ComidaDia { NombreComida = "Merienda", Hora = "16:30", Calorias = 250, Proteinas = 28, Carbohidratos = 20, Grasas = 7,
                        Alimentos = new List<string> { "100g queso cottage", "2 tortitas de arroz", "Verduras crudas" } },
                    new ComidaDia { NombreComida = "Cena", Hora = "20:00", Calorias = 500, Proteinas = 42, Carbohidratos = 35, Grasas = 16,
                        Alimentos = new List<string> { "200g pescado blanco o salmon", "120g batata al horno", "Vegetales al vapor abundantes", "Aceite de oliva extra virgen" } }
                },
                RecomendacionesGenerales = new List<string>
                {
                    "Los dias de entrenamiento aumentar carbohidratos en 50-80g",
                    "Los dias de descanso reducir carbohidratos y aumentar grasas saludables",
                    "Mantener proteina constante todos los dias (minimo 2g por kg de peso)",
                    "Hidratarse con minimo 2.5 litros de agua",
                    "Controlar porciones con la palma de la mano como referencia"
                }
            };
        }
    }
}
