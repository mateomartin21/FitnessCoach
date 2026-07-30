using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Models.Objetivos;

namespace FitnessCoach.Tests.Fakes
{
    /// <summary>Rutina fija de dos días: alcanza para saber qué etiquetas son válidas.</summary>
    public class GeneradorRutinasFalso : IGeneradorRutinas
    {
        /// <summary>Las etiquetas que forma <c>ServicioEntrenamientos</c> con estos días.</summary>
        public const string Dia1 = "Día 1 — Piernas";
        public const string Dia2 = "Día 2 — Torso";

        public Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo, int semillaRotacion = 0) => new()
        {
            NombreRutina = "Rutina de prueba",
            Dias = new List<DiaEntrenamiento>
            {
                new() { NombreDia = "Día 1", Enfoque = "Piernas" },
                new() { NombreDia = "Día 2", Enfoque = "Torso" }
            }
        };
    }
}
