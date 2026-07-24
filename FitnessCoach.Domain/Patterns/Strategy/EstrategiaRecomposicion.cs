using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Domain.Patterns.Strategy
{
    public class EstrategiaRecomposicion : EstrategiaRutinaBase
    {
        public EstrategiaRecomposicion(IRepositorioEjercicios catalogo, int semillaRotacion = 0)
            : base(catalogo, semillaRotacion) { }

        protected override string NombreRutina => "Recomposición Estructural: Torso/Pierna (4 Días)";
        protected override string Nivel => "Intermedio";

        protected override IReadOnlyList<string> EquiposPreferidos =>
            new[] { "barbell", "dumbbell", "cable", "bodyweight", "lever" };

        protected override IReadOnlyList<PlantillaDia> Plan => new PlantillaDia[]
        {
            new()
            {
                NombreDia = "Lunes",
                Enfoque = "Torso Fuerza",
                Bloques = new BloqueEjercicios[]
                {
                    new("pectorals", 2, 4, "5-8"),
                    new("upper-back", 1, 4, "6-8"),
                    new("delts", 1, 3, "8-10")
                }
            },
            new()
            {
                NombreDia = "Martes",
                Enfoque = "Pierna Fuerza",
                Bloques = new BloqueEjercicios[]
                {
                    new("quads", 2, 4, "5-8"),
                    new("hamstrings", 1, 4, "8-10"),
                    new("glutes", 1, 3, "10-12")
                }
            },
            new()
            {
                NombreDia = "Jueves",
                Enfoque = "Torso Hipertrofia",
                Bloques = new BloqueEjercicios[]
                {
                    new("pectorals", 1, 4, "10-12"),
                    new("delts", 2, 4, "15-20"),
                    new("biceps", 1, 3, "12-15"),
                    new("triceps", 1, 3, "12-15")
                }
            },
            new()
            {
                NombreDia = "Viernes",
                Enfoque = "Pierna Hipertrofia",
                Bloques = new BloqueEjercicios[]
                {
                    new("quads", 1, 3, "10-12 por pierna"),
                    new("glutes", 2, 4, "10-12"),
                    new("hamstrings", 1, 3, "15"),
                    new("calves", 1, 3, "15-20")
                }
            }
        };
    }
}
