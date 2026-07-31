using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.ServicesIntegration
{
    public class GeneradorAlimentacionServiceTests
    {
        private static GeneradorAlimentacionService Generador() =>
            new(RepositorioAlimentosFalso.ConCatalogoDePrueba(), new CalculadorCaloricoService());

        private static UsuarioPerfil Usuario(ObjetivoFitness objetivo, double pesoKg = 70, int id = 1) => new()
        {
            Id = id,
            Nombre = "Ana",
            Edad = 30,
            EstaturaCm = 165,
            PesoKg = pesoKg,
            ObjetivoActual = objetivo
        };

        [Theory]
        [InlineData(typeof(ObjetivoPerderPeso), "Pérdida de Grasa")]
        [InlineData(typeof(ObjetivoGanarMusculo), "Ganancia Muscular")]
        [InlineData(typeof(ObjetivoRecomposicion), "Recomposición y Fuerza")]
        public void CadaObjetivo_SeleccionaSuEstrategia(Type tipoObjetivo, string objetivoEsperado)
        {
            var objetivo = (ObjetivoFitness)Activator.CreateInstance(tipoObjetivo)!;

            var plan = Generador().GenerarPlanPara(Usuario(objetivo));

            Assert.Equal(objetivoEsperado, plan.Objetivo);
        }

        [Fact]
        public void SiempreIncluyeLasRecomendacionesDeHidratacion()
        {
            var plan = Generador().GenerarPlanPara(Usuario(new ObjetivoRecomposicion()));

            Assert.Contains("Tomar 500ml de agua al despertar en ayunas", plan.RecomendacionesGenerales);
        }

        [Fact]
        public void ElPlanSeAdaptaAlPeso_NoEsElMismoParaTodos()
        {
            // Es la razón de ser de toda la fase: antes, dos personas con el mismo
            // objetivo recibían exactamente el mismo plan y las mismas calorías fijas.
            var liviano = Generador().GenerarPlanPara(Usuario(new ObjetivoGanarMusculo(), pesoKg: 55));
            var pesado = Generador().GenerarPlanPara(Usuario(new ObjetivoGanarMusculo(), pesoKg: 95));

            Assert.True(pesado.Objetivos.Calorias > liviano.Objetivos.Calorias);
            Assert.True(pesado.Objetivos.ProteinaG > liviano.Objetivos.ProteinaG);
            Assert.True(pesado.CaloriasTotales > liviano.CaloriasTotales);
        }

        [Fact]
        public void ElPlanEsEstable_ElMismoUsuarioVeSiempreLoMismo()
        {
            // Si la selección fuera aleatoria, el plan cambiaría al refrescar la página.
            var usuario = Usuario(new ObjetivoPerderPeso());

            var primero = Generador().GenerarPlanPara(usuario);
            var segundo = Generador().GenerarPlanPara(usuario);

            Assert.Equal(
                primero.Comidas.SelectMany(c => c.Alimentos),
                segundo.Comidas.SelectMany(c => c.Alimentos));
        }

        [Fact]
        public void DosUsuariosDistintos_NoRecibenExactamenteElMismoPlan()
        {
            var ana = Usuario(new ObjetivoPerderPeso(), id: 1);
            var beto = Usuario(new ObjetivoPerderPeso(), id: 2);

            var planAna = Generador().GenerarPlanPara(ana);
            var planBeto = Generador().GenerarPlanPara(beto);

            Assert.NotEqual(
                planAna.Comidas.SelectMany(c => c.Alimentos),
                planBeto.Comidas.SelectMany(c => c.Alimentos));
        }

        [Fact]
        public void SinUsuario_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => Generador().GenerarPlanPara(null!));
        }
    }
}
