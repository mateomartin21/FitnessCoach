using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Domain.Patterns.Strategy
{
    public class EstrategiaPerderPeso : EstrategiaRutinaBase
    {
        public EstrategiaPerderPeso(IRepositorioEjercicios catalogo, int semillaRotacion = 0,
                                 PreferenciasEntrenamiento? preferencias = null)
            : base(catalogo, semillaRotacion, preferencias) { }

        protected override string NombreRutina => "Quema de Grasa: Full Body Activo (3 Días)";
        protected override string Nivel => "Principiante/Intermedio";

        // Principiantes: peso corporal y mancuerna antes que barra libre, que exige técnica.
        protected override IReadOnlyList<string> EquiposPreferidos =>
            new[] { "bodyweight", "dumbbell", "band", "kettlebell", "cable" };

        protected override IReadOnlyList<PlantillaDia> Plan => new PlantillaDia[]
        {
            new()
            {
                NombreDia = "Lunes",
                Enfoque = "Cuerpo Completo + Cardio",
                Bloques = new BloqueEjercicios[]
                {
                    new("quads", 2, 4, "12-15"),
                    new("pectorals", 1, 3, "Al fallo"),
                    new("upper-back", 1, 3, "12-15"),
                    new("abs", 1, 3, "45 seg"),
                    new("cardio", 1, 1, "25 min")
                }
            },
            new()
            {
                NombreDia = "Miércoles",
                Enfoque = "Cuerpo Completo B + Cardio",
                Bloques = new BloqueEjercicios[]
                {
                    new("glutes", 2, 3, "12 por pierna"),
                    new("delts", 1, 3, "12-15"),
                    new("lats", 1, 3, "15"),
                    new("cardio", 1, 1, "25 min")
                }
            },
            new()
            {
                NombreDia = "Viernes",
                Enfoque = "Circuito Metabólico",
                Bloques = new BloqueEjercicios[]
                {
                    new("quads", 2, 4, "15"),
                    new("abs", 2, 4, "30 seg"),
                    new("cardio", 1, 1, "20 min")
                }
            }
        };
    }
}
