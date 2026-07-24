using System.Collections.Generic;
using FitnessCoach.Domain.Models.Entrenamiento;
using FitnessCoach.Domain.Patterns.Decorator;
using FitnessCoach.Domain.Patterns.Strategy;
using Xunit;

namespace FitnessCoach.Tests.Patterns
{
    // Estrategia falsa y controlada, solo para estas pruebas
    public class EstrategiaFalsa : IEstrategiaRutina
    {
        public Rutina GenerarRutina()
        {
            return new Rutina
            {
                NombreRutina = "Rutina de Prueba",
                Nivel = "Intermedio",
                Dias = new List<DiaEntrenamiento>
                {
                    new DiaEntrenamiento
                    {
                        NombreDia = "Lunes",
                        Enfoque = "Prueba",
                        Ejercicios = new List<EjercicioPrescrito>
                        {
                            new EjercicioPrescrito
                            {
                                Ejercicio = new Ejercicio { Slug = "original", Nombre = "Ejercicio Original" },
                                Series = 3,
                                Repeticiones = "10"
                            }
                        }
                    }
                }
            };
        }
    }

    public class RutinaDecoratorTests
    {
        [Fact]
        public void RutinaConCalentamiento_InsertaCalentamientoAlInicioDeCadaDia()
        {
            // Arrange
            var decorator = new RutinaConCalentamiento(new EstrategiaFalsa());

            // Act
            var rutina = decorator.GenerarRutina();

            // Assert
            var dia = rutina.Dias[0];
            Assert.Equal("Calentamiento General", dia.Ejercicios[0].Nombre);
            Assert.Equal("Ejercicio Original", dia.Ejercicios[1].Nombre);
            Assert.Equal(2, dia.Ejercicios.Count);
        }

        [Fact]
        public void RutinaConEnfriamiento_AgregaEnfriamientoAlFinalDeCadaDia()
        {
            // Arrange
            var decorator = new RutinaConEnfriamiento(new EstrategiaFalsa());

            // Act
            var rutina = decorator.GenerarRutina();

            // Assert
            var dia = rutina.Dias[0];
            Assert.Equal("Ejercicio Original", dia.Ejercicios[0].Nombre);
            Assert.Equal("Enfriamiento y Estiramientos", dia.Ejercicios[1].Nombre);
            Assert.Equal(2, dia.Ejercicios.Count);
        }

        [Fact]
        public void CalentamientoYEnfriamiento_CombinadosMantienenElOrdenCorrecto()
        {
            // Arrange — igual que lo hace GeneradorRutinasService:
            // RutinaConEnfriamiento envolviendo a RutinaConCalentamiento
            var decorator = new RutinaConEnfriamiento(new RutinaConCalentamiento(new EstrategiaFalsa()));

            // Act
            var rutina = decorator.GenerarRutina();

            // Assert
            var ejercicios = rutina.Dias[0].Ejercicios;
            Assert.Equal(3, ejercicios.Count);
            Assert.Equal("Calentamiento General", ejercicios[0].Nombre);
            Assert.Equal("Ejercicio Original", ejercicios[1].Nombre);
            Assert.Equal("Enfriamiento y Estiramientos", ejercicios[2].Nombre);
        }
    }
}