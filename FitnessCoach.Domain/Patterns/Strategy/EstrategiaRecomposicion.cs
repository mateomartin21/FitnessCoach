using FitnessCoach.Domain.Models.Entrenamiento;

namespace FitnessCoach.Domain.Patterns.Strategy
{
    public class EstrategiaRecomposicion : IEstrategiaRutina
    {
        public Rutina GenerarRutina()
        {
            var rutina = new Rutina { NombreRutina = "Recomposición Estructural: Torso/Pierna (4 Días)", Nivel = "Intermedio" };
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Lunes", Enfoque = "Torso Fuerza", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Press de Banca con Barra", Series = 4, Repeticiones = "5-8" },
                new Ejercicio { Nombre = "Remo con Barra", Series = 4, Repeticiones = "6-8" },
                new Ejercicio { Nombre = "Press Militar", Series = 3, Repeticiones = "8-10" }
            }});
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Martes", Enfoque = "Pierna Fuerza", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Sentadilla Libre", Series = 4, Repeticiones = "5-8" },
                new Ejercicio { Nombre = "Peso Muerto Rumano", Series = 4, Repeticiones = "8-10" },
                new Ejercicio { Nombre = "Prensa de Piernas", Series = 3, Repeticiones = "10-12" }
            }});
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Jueves", Enfoque = "Torso Hipertrofia", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Press Inclinado Mancuernas", Series = 4, Repeticiones = "10-12" },
                new Ejercicio { Nombre = "Elevaciones Laterales", Series = 4, Repeticiones = "15-20" },
                new Ejercicio { Nombre = "Curl Martillo", Series = 3, Repeticiones = "12-15" }
            }});
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Viernes", Enfoque = "Pierna Hipertrofia", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Sentadilla Búlgara", Series = 3, Repeticiones = "10-12 por pierna" },
                new Ejercicio { Nombre = "Hip Thrust", Series = 4, Repeticiones = "10-12" },
                new Ejercicio { Nombre = "Curl de Isquios", Series = 3, Repeticiones = "15" }
            }});
            return rutina;
        }
    }
}
