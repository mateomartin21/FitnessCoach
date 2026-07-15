using FitnessCoach.Domain.Models.Alimentacion;

namespace FitnessCoach.Domain.Patterns.Strategy.Alimentacion
{
    public class AlimentacionGanarMusculo : IEstrategiaAlimentacion
    {
        public PlanAlimentacion GenerarPlan()
        {
            return new PlanAlimentacion
            {
                NombrePlan = "Plan Superavit Calorico",
                Objetivo = "Ganancia Muscular",
                CaloriasObjetivo = "2800-3200 kcal/dia",
                Descripcion = "Alto en proteina y carbohidratos complejos. Superavit de 300-400 kcal para maximizar sintesis proteica.",
                Comidas = new List<ComidaDia>
                {
                    new ComidaDia { NombreComida = "Desayuno", Hora = "07:00", Calorias = 650, Proteinas = 40, Carbohidratos = 80, Grasas = 15,
                        Alimentos = new List<string> { "3 huevos enteros + 3 claras revueltos", "100g avena con leche entera", "1 platano maduro", "30g mantequilla de mani natural" } },
                    new ComidaDia { NombreComida = "Snack Manana", Hora = "10:00", Calorias = 350, Proteinas = 30, Carbohidratos = 40, Grasas = 8,
                        Alimentos = new List<string> { "Batido: 250ml leche + 30g proteina whey + 50g avena + 1 platano" } },
                    new ComidaDia { NombreComida = "Almuerzo", Hora = "13:00", Calorias = 750, Proteinas = 55, Carbohidratos = 80, Grasas = 18,
                        Alimentos = new List<string> { "220g pechuga o muslo de pollo", "180g arroz blanco cocido", "150g frijoles negros", "Aguacate 1/2 unidad", "Vegetales salteados" } },
                    new ComidaDia { NombreComida = "Pre-Entreno", Hora = "16:00", Calorias = 300, Proteinas = 25, Carbohidratos = 40, Grasas = 5,
                        Alimentos = new List<string> { "2 tostadas integrales con atun", "1 fruta de temporada", "Cafe negro sin azucar" } },
                    new ComidaDia { NombreComida = "Post-Entreno", Hora = "18:30", Calorias = 280, Proteinas = 35, Carbohidratos = 30, Grasas = 3,
                        Alimentos = new List<string> { "30g proteina whey con agua", "1 platano o 50g arroz blanco" } },
                    new ComidaDia { NombreComida = "Cena", Hora = "21:00", Calorias = 620, Proteinas = 50, Carbohidratos = 50, Grasas = 20,
                        Alimentos = new List<string> { "250g carne de res magra o salmon", "150g papa cocida", "Ensalada completa con aceite de oliva", "1 vaso leche entera antes de dormir" } }
                },
                RecomendacionesGenerales = new List<string>
                {
                    "Consumir proteina dentro de los 30 min post-entreno",
                    "Distribuir la proteina en 5-6 comidas al dia",
                    "Priorizar carbohidratos complejos antes del entrenamiento",
                    "No descuidar las grasas saludables para produccion hormonal",
                    "Dormir minimo 8 horas para maximizar la recuperacion muscular"
                }
            };
        }
    }
}
