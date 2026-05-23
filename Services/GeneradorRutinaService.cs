using FitnessCoach.Models.Entrenamiento;
using FitnessCoach.Models.Objetivos;

namespace FitnessCoach.Services
{
    public class GeneradorRutinasService : IGeneradorRutinas
    {
        public Rutina GenerarRutinaParaObjetivo(ObjetivoFitness objetivo)
        {
            return objetivo switch
            {
                ObjetivoPerderPeso => GenerarRutinaPerdidaPeso(),
                ObjetivoRecomposicion => GenerarRutinaRecomposicion(),
                ObjetivoGanarMusculo => GenerarRutinaHipertrofia(),
                _ => GenerarRutinaGeneral()
            };
        }

        // --------------------------------------------------------
        // 1. RUTINA PARA PÉRDIDA DE PESO (3 Días Full Body + Cardio)
        // --------------------------------------------------------
        private Rutina GenerarRutinaPerdidaPeso()
        {
            var rutina = new Rutina { NombreRutina = "Quema de Grasa: Full Body Activo (3 Días)", Nivel = "Principiante/Intermedio" };

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Lunes",
                Enfoque = "Cuerpo Completo A + Cardio",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Sentadilla con Copa (Goblet Squat)", Series = 4, Repeticiones = "12-15", Notas = "Controla la bajada" },
                    new Ejercicio { Nombre = "Flexiones (Push-ups) o en rodillas", Series = 3, Repeticiones = "Al fallo" },
                    new Ejercicio { Nombre = "Remo con Mancuernas (Apoyado)", Series = 3, Repeticiones = "12-15" },
                    new Ejercicio { Nombre = "Peso Muerto Rumano con Mancuernas", Series = 3, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Plancha Abdominal (Plank)", Series = 3, Repeticiones = "45-60 seg" },
                    new Ejercicio { Nombre = "Caminadora (Inclinación Alta)", Series = 1, Repeticiones = "25 minutos", Notas = "Cardio LISS (Ritmo constante)" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Miércoles",
                Enfoque = "Cuerpo Completo B + Cardio",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Zancadas Inversas (Lunges)", Series = 3, Repeticiones = "12 por pierna" },
                    new Ejercicio { Nombre = "Press Militar con Mancuernas", Series = 3, Repeticiones = "12-15" },
                    new Ejercicio { Nombre = "Jalón al pecho agarre neutro", Series = 3, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Puente de Glúteo (Hip Thrust sin peso)", Series = 4, Repeticiones = "20", Notas = "Aprieta arriba 2 segundos" },
                    new Ejercicio { Nombre = "Crunch Abdominal en polea o piso", Series = 3, Repeticiones = "15-20" },
                    new Ejercicio { Nombre = "Bicicleta Estática o Elíptica", Series = 1, Repeticiones = "25 minutos", Notas = "Cardio LISS" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Viernes",
                Enfoque = "Circuito Metabólico",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Sentadillas con Salto (Jump Squats)", Series = 4, Repeticiones = "15", Notas = "Sin descanso entre ejercicios del circuito" },
                    new Ejercicio { Nombre = "Mountain Climbers", Series = 4, Repeticiones = "30 seg" },
                    new Ejercicio { Nombre = "Remo en TRX o con banda", Series = 4, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Swing con Pesa Rusa (Kettlebell)", Series = 4, Repeticiones = "20" },
                    new Ejercicio { Nombre = "Burpees", Series = 4, Repeticiones = "10-12" },
                    new Ejercicio { Nombre = "Descanso activo (Caminata ligera)", Series = 4, Repeticiones = "90 seg", Notas = "Al finalizar cada vuelta" }
                }
            });

            return rutina;
        }

        // --------------------------------------------------------
        // 2. RUTINA PARA RECOMPOSICIÓN (4 Días Torso/Pierna)
        // --------------------------------------------------------
        private Rutina GenerarRutinaRecomposicion()
        {
            var rutina = new Rutina { NombreRutina = "Recomposición Estructural: Torso/Pierna (4 Días)", Nivel = "Intermedio" };

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Lunes",
                Enfoque = "Torso (Enfoque Fuerza)",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Press de Banca con Barra", Series = 4, Repeticiones = "5-8", Notas = "Descanso de 2-3 min" },
                    new Ejercicio { Nombre = "Remo con Barra o Pendlay", Series = 4, Repeticiones = "6-8" },
                    new Ejercicio { Nombre = "Press Militar de pie", Series = 3, Repeticiones = "8-10" },
                    new Ejercicio { Nombre = "Dominadas o Jalón al pecho", Series = 3, Repeticiones = "8-10" },
                    new Ejercicio { Nombre = "Extensión de Tríceps en Polea", Series = 3, Repeticiones = "12-15" },
                    new Ejercicio { Nombre = "Curl de Bíceps con Barra EZ", Series = 3, Repeticiones = "12-15" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Martes",
                Enfoque = "Pierna (Enfoque Fuerza)",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Sentadilla Libre con Barra", Series = 4, Repeticiones = "5-8", Notas = "Profundidad rompiendo el paralelo" },
                    new Ejercicio { Nombre = "Peso Muerto Rumano", Series = 4, Repeticiones = "8-10" },
                    new Ejercicio { Nombre = "Prensa de Piernas (Leg Press)", Series = 3, Repeticiones = "10-12" },
                    new Ejercicio { Nombre = "Curl de Isquios acostado", Series = 3, Repeticiones = "10-12" },
                    new Ejercicio { Nombre = "Elevación de Talones de pie", Series = 4, Repeticiones = "12-15" },
                    new Ejercicio { Nombre = "Rueda Abdominal (Ab Wheel)", Series = 3, Repeticiones = "10-12" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Jueves",
                Enfoque = "Torso (Enfoque Hipertrofia)",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Press Inclinado con Mancuernas", Series = 4, Repeticiones = "10-12" },
                    new Ejercicio { Nombre = "Remo unilateral en polea o mancuerna", Series = 4, Repeticiones = "10-12 por brazo" },
                    new Ejercicio { Nombre = "Elevaciones Laterales para Hombro", Series = 4, Repeticiones = "15-20" },
                    new Ejercicio { Nombre = "Face Pulls (Deltoides posterior)", Series = 3, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Curl Martillo con Mancuernas", Series = 3, Repeticiones = "12-15" },
                    new Ejercicio { Nombre = "Press Francés o Copa Tríceps", Series = 3, Repeticiones = "12-15" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Viernes",
                Enfoque = "Pierna (Enfoque Hipertrofia)",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Sentadilla Búlgara", Series = 3, Repeticiones = "10-12 por pierna", Notas = "Apoyo en banco" },
                    new Ejercicio { Nombre = "Hip Thrust con Barra", Series = 4, Repeticiones = "10-12", Notas = "Pausa de 1 seg arriba" },
                    new Ejercicio { Nombre = "Extensión de Cuádriceps en máquina", Series = 3, Repeticiones = "15", Notas = "Al fallo en la última serie" },
                    new Ejercicio { Nombre = "Curl de Isquios sentado", Series = 3, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Elevación de Talones sentado", Series = 4, Repeticiones = "15-20" },
                    new Ejercicio { Nombre = "Plancha con peso o declinada", Series = 3, Repeticiones = "60 seg" }
                }
            });

            return rutina;
        }

        // --------------------------------------------------------
        // 3. RUTINA PARA GANANCIA MUSCULAR (5 Días Bro-Split / Push-Pull-Legs adaptado)
        // --------------------------------------------------------
        private Rutina GenerarRutinaHipertrofia()
        {
            var rutina = new Rutina { NombreRutina = "Hipertrofia Máxima (5 Días)", Nivel = "Avanzado" };

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Lunes",
                Enfoque = "Pecho y Tríceps",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Press Inclinado con Mancuernas", Series = 4, Repeticiones = "8-10" },
                    new Ejercicio { Nombre = "Press de Banca Plano con Barra", Series = 3, Repeticiones = "8-12" },
                    new Ejercicio { Nombre = "Aperturas en Polea (Crossover)", Series = 4, Repeticiones = "12-15", Notas = "Enfócate en el estiramiento" },
                    new Ejercicio { Nombre = "Fondos en Paralelas (Dips)", Series = 3, Repeticiones = "Al fallo" },
                    new Ejercicio { Nombre = "Rompecráneos (Skull Crushers)", Series = 4, Repeticiones = "10-12" },
                    new Ejercicio { Nombre = "Extensión de Tríceps con Cuerda", Series = 3, Repeticiones = "15" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Martes",
                Enfoque = "Espalda y Bíceps",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Dominadas lastradas o libres", Series = 4, Repeticiones = "6-10" },
                    new Ejercicio { Nombre = "Remo Gironda (Polea Baja)", Series = 4, Repeticiones = "10-12" },
                    new Ejercicio { Nombre = "Jalón al pecho unilateral", Series = 3, Repeticiones = "12 por brazo" },
                    new Ejercicio { Nombre = "Pull-over en Polea Alta", Series = 3, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Curl de Bíceps con Barra Recta", Series = 4, Repeticiones = "8-10" },
                    new Ejercicio { Nombre = "Curl Predicador en Máquina", Series = 3, Repeticiones = "12-15" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Miércoles",
                Enfoque = "Piernas Completas",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Sentadilla Hack o Libre", Series = 4, Repeticiones = "8-10" },
                    new Ejercicio { Nombre = "Prensa de Piernas (Pies bajos)", Series = 4, Repeticiones = "12-15" },
                    new Ejercicio { Nombre = "Extensión de Piernas", Series = 3, Repeticiones = "15-20", Notas = "Drop set en la última serie" },
                    new Ejercicio { Nombre = "Peso Muerto Piernas Rígidas", Series = 4, Repeticiones = "10" },
                    new Ejercicio { Nombre = "Curl Femoral Tumbado", Series = 3, Repeticiones = "12-15" },
                    new Ejercicio { Nombre = "Elevación de Talones en Máquina", Series = 5, Repeticiones = "15-20" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Jueves",
                Enfoque = "Hombros y Trapecios",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Press Militar Sentado con Mancuernas", Series = 4, Repeticiones = "8-10" },
                    new Ejercicio { Nombre = "Elevaciones Laterales con Mancuernas", Series = 4, Repeticiones = "15-20", Notas = "Movimiento estricto" },
                    new Ejercicio { Nombre = "Elevaciones Laterales en Polea", Series = 3, Repeticiones = "12-15 por brazo" },
                    new Ejercicio { Nombre = "Pájaros (Reverse Pec Deck)", Series = 4, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Encogimientos de Hombros (Shrugs)", Series = 4, Repeticiones = "15-20" },
                    new Ejercicio { Nombre = "Face Pulls", Series = 3, Repeticiones = "15" }
                }
            });

            rutina.Dias.Add(new DiaEntrenamiento
            {
                NombreDia = "Viernes",
                Enfoque = "Brazos (Pump Day) y Abdomen",
                Ejercicios = new List<Ejercicio> {
                    new Ejercicio { Nombre = "Curl de Bíceps en Banco Inclinado", Series = 4, Repeticiones = "12" },
                    new Ejercicio { Nombre = "Extensión de Tríceps tras nuca", Series = 4, Repeticiones = "12" },
                    new Ejercicio { Nombre = "Curl Martillo con Cuerda en Polea", Series = 3, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Kickback de Tríceps en Polea", Series = 3, Repeticiones = "15" },
                    new Ejercicio { Nombre = "Elevación de Piernas Colgado", Series = 4, Repeticiones = "12-15" },
                    new Ejercicio { Nombre = "Crunch en Polea Alta", Series = 4, Repeticiones = "15-20" }
                }
            });

            return rutina;
        }

        private Rutina GenerarRutinaGeneral()
        {
            return new Rutina { NombreRutina = "Acondicionamiento Básico" };
        }
    }
}