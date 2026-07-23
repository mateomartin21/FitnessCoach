using System.Collections.Generic;
using FitnessCoach.Domain.Models.Alimentacion;
using FitnessCoach.Domain.Patterns.Decorator;
using FitnessCoach.Domain.Patterns.Strategy.Alimentacion;
using Xunit;

namespace FitnessCoach.Tests.Patterns
{
    public class EstrategiaAlimentacionFalsa : IEstrategiaAlimentacion
    {
        public PlanAlimentacion GenerarPlan()
        {
            return new PlanAlimentacion
            {
                NombrePlan = "Plan de Prueba",
                Objetivo = "Prueba",
                CaloriasObjetivo = "2000",
                Descripcion = "Plan falso para pruebas",
                Comidas = new List<ComidaDia>(),
                RecomendacionesGenerales = new List<string> { "Recomendación original" }
            };
        }
    }

    public class PlanConHidratacionTests
    {
        [Fact]
        public void GenerarPlan_AgregaLasTresRecomendacionesDeHidratacion()
        {
            // Arrange
            var decorator = new PlanConHidratacion(new EstrategiaAlimentacionFalsa());

            // Act
            var plan = decorator.GenerarPlan();

            // Assert
            Assert.Equal(4, plan.RecomendacionesGenerales.Count); // 1 original + 3 de hidratación
            Assert.Contains("Tomar 500ml de agua al despertar en ayunas", plan.RecomendacionesGenerales);
            Assert.Contains("Beber 250ml de agua 30 minutos antes de cada comida", plan.RecomendacionesGenerales);
            Assert.Contains("Evitar bebidas azucaradas y alcohol durante el plan", plan.RecomendacionesGenerales);
        }

        [Fact]
        public void GenerarPlan_NoModificaElRestoDelPlanOriginal()
        {
            // Arrange
            var decorator = new PlanConHidratacion(new EstrategiaAlimentacionFalsa());

            // Act
            var plan = decorator.GenerarPlan();

            // Assert — el decorator solo debe tocar RecomendacionesGenerales
            Assert.Equal("Plan de Prueba", plan.NombrePlan);
            Assert.Equal("2000", plan.CaloriasObjetivo);
        }
    }
}