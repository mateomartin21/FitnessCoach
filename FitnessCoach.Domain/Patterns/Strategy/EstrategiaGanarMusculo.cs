using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Ports;

namespace FitnessCoach.Domain.Patterns.Strategy
{
    public class EstrategiaGanarMusculo : EstrategiaRutinaBase
    {
        public EstrategiaGanarMusculo(IRepositorioEjercicios catalogo, int semillaRotacion = 0,
                                 PreferenciasEntrenamiento? preferencias = null)
            : base(catalogo, semillaRotacion, preferencias) { }

        protected override string NombreRutina => "Hipertrofia Máxima (5 Días)";
        protected override string Nivel => "Avanzado";

        // Hipertrofia: cargas altas, así que barra y mancuerna antes que banda elástica.
        protected override IReadOnlyList<string> EquiposPreferidos =>
            new[] { "barbell", "dumbbell", "cable", "ez-bar", "lever" };

        protected override IReadOnlyList<PlantillaDia> Plan => new PlantillaDia[]
        {
            new()
            {
                NombreDia = "Lunes",
                Enfoque = "Pecho y Tríceps",
                Bloques = new BloqueEjercicios[]
                {
                    new("pectorals", 3, 4, "8-10"),
                    new("triceps", 2, 4, "10-12")
                }
            },
            new()
            {
                NombreDia = "Martes",
                Enfoque = "Espalda y Bíceps",
                Bloques = new BloqueEjercicios[]
                {
                    new("lats", 2, 4, "8-10"),
                    new("upper-back", 1, 4, "10-12"),
                    new("biceps", 2, 4, "8-10")
                }
            },
            new()
            {
                NombreDia = "Miércoles",
                Enfoque = "Piernas",
                Bloques = new BloqueEjercicios[]
                {
                    new("quads", 2, 4, "8-10"),
                    new("hamstrings", 2, 4, "10-12"),
                    new("calves", 1, 4, "15-20")
                }
            },
            new()
            {
                NombreDia = "Jueves",
                Enfoque = "Hombros y Trapecio",
                Bloques = new BloqueEjercicios[]
                {
                    new("delts", 3, 4, "10-12"),
                    new("traps", 1, 3, "12-15")
                }
            },
            new()
            {
                NombreDia = "Viernes",
                Enfoque = "Glúteos y Core",
                Bloques = new BloqueEjercicios[]
                {
                    new("glutes", 2, 4, "10-12"),
                    new("abs", 2, 3, "15-20")
                }
            }
        };
    }
}
