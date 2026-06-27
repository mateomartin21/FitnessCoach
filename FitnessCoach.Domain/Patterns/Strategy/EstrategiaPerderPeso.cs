using FitnessCoach.Domain.Models.Entrenamiento;

namespace FitnessCoach.Domain.Patterns.Strategy
{
    public class EstrategiaPerderPeso : IEstrategiaRutina
    {
        public Rutina GenerarRutina()
        {
            var rutina = new Rutina { NombreRutina = "Quema de Grasa: Full Body Activo (3 Días)", Nivel = "Principiante/Intermedio" };
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Lunes", Enfoque = "Cuerpo Completo + Cardio", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Sentadilla Goblet", Series = 4, Repeticiones = "12-15" },
                new Ejercicio { Nombre = "Flexiones", Series = 3, Repeticiones = "Al fallo" },
                new Ejercicio { Nombre = "Remo con Mancuernas", Series = 3, Repeticiones = "12-15" },
                new Ejercicio { Nombre = "Plancha", Series = 3, Repeticiones = "45 seg" },
                new Ejercicio { Nombre = "Caminadora Inclinada", Series = 1, Repeticiones = "25 min" }
            }});
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Miércoles", Enfoque = "Cuerpo Completo B + Cardio", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Zancadas Inversas", Series = 3, Repeticiones = "12 por pierna" },
                new Ejercicio { Nombre = "Press Militar", Series = 3, Repeticiones = "12-15" },
                new Ejercicio { Nombre = "Jalón al Pecho", Series = 3, Repeticiones = "15" },
                new Ejercicio { Nombre = "Bicicleta Estática", Series = 1, Repeticiones = "25 min" }
            }});
            rutina.Dias.Add(new DiaEntrenamiento { NombreDia = "Viernes", Enfoque = "Circuito Metabólico", Ejercicios = new List<Ejercicio> {
                new Ejercicio { Nombre = "Sentadillas con Salto", Series = 4, Repeticiones = "15" },
                new Ejercicio { Nombre = "Mountain Climbers", Series = 4, Repeticiones = "30 seg" },
                new Ejercicio { Nombre = "Burpees", Series = 4, Repeticiones = "10-12" }
            }});
            return rutina;
        }
    }
}
