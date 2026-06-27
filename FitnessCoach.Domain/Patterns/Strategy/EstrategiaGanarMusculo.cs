using FitnessCoach.Domain.Models.Entrenamiento;

namespace FitnessCoach.Domain.Patterns.Strategy
{
    public class EstrategiaGanarMusculo : IEstrategiaRutina
    {
        public Rutina GenerarRutina()
        {
            var rutina = new Rutina { NombreRutina = "Hipertrofia Máxima (5 Días)", Nivel = "Avanzado" };
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Lunes", Enfoque = "Pecho y Tríceps", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Press Inclinado con Mancuernas", Series = 4, Repeticiones = "8-10" },
                new Ejercicio { Nombre = "Press de Banca", Series = 3, Repeticiones = "8-12" },
                new Ejercicio { Nombre = "Fondos en Paralelas", Series = 3, Repeticiones = "Al fallo" },
                new Ejercicio { Nombre = "Rompecráneos", Series = 4, Repeticiones = "10-12" }
            }});
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Martes", Enfoque = "Espalda y Bíceps", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Dominadas", Series = 4, Repeticiones = "6-10" },
                new Ejercicio { Nombre = "Remo con Barra", Series = 4, Repeticiones = "10-12" },
                new Ejercicio { Nombre = "Curl de Bíceps", Series = 4, Repeticiones = "8-10" }
            }});
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Jueves", Enfoque = "Piernas", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Sentadilla Libre", Series = 4, Repeticiones = "8-10" },
                new Ejercicio { Nombre = "Prensa de Piernas", Series = 4, Repeticiones = "12-15" },
                new Ejercicio { Nombre = "Peso Muerto Rumano", Series = 4, Repeticiones = "10" }
            }});
            return rutina;
        }
    }
}
