using FitnessCoach.Domain.Models.Gamificacion;
using Xunit;

namespace FitnessCoach.Tests.Gamificacion
{
    public class CalculadorNivelTests
    {
        [Fact]
        public void SinXp_EsNivel1ReciénEmpezado()
        {
            var nivel = CalculadorNivel.Calcular(0);

            Assert.Equal(1, nivel.Numero);
            Assert.Equal(0, nivel.XpEnNivel);
            Assert.Equal(100, nivel.XpParaSubir);   // costo del 1 → 2
            Assert.Equal(0, nivel.PorcentajeAvance);
        }

        [Fact]
        public void JustoAntesDeSubir_SigueEnElMismoNivel()
        {
            var nivel = CalculadorNivel.Calcular(99);

            Assert.Equal(1, nivel.Numero);
            Assert.Equal(99, nivel.XpEnNivel);
            Assert.Equal(1, nivel.XpRestante);
        }

        [Fact]
        public void AlCompletarElCosto_SubeDeNivel()
        {
            var nivel = CalculadorNivel.Calcular(100);

            Assert.Equal(2, nivel.Numero);
            Assert.Equal(0, nivel.XpEnNivel);
            Assert.Equal(150, nivel.XpParaSubir);   // el nivel 2 → 3 cuesta más
        }

        [Fact]
        public void LaCurvaEsCreciente_CadaNivelCuestaMas()
        {
            // 100 (1→2) + 150 (2→3) = 250 justo para llegar al nivel 3.
            var nivel = CalculadorNivel.Calcular(250);

            Assert.Equal(3, nivel.Numero);
            Assert.Equal(0, nivel.XpEnNivel);
            Assert.Equal(200, nivel.XpParaSubir);
        }

        [Fact]
        public void LaBarraReflejaElAvanceDentroDelNivel()
        {
            // Nivel 1 cuesta 100; con 50 encima, la barra va a la mitad.
            var nivel = CalculadorNivel.Calcular(50);

            Assert.Equal(1, nivel.Numero);
            Assert.Equal(50, nivel.PorcentajeAvance);
        }

        [Theory]
        [InlineData(0, 1, "Cachorro")]
        [InlineData(-500, 1, "Cachorro")]   // XP negativo no rompe: se trata como 0
        public void XpNoPositivo_CaeEnNivel1(int xp, int numeroEsperado, string titulo)
        {
            var nivel = CalculadorNivel.Calcular(xp);

            Assert.Equal(numeroEsperado, nivel.Numero);
            Assert.Equal(titulo, nivel.Titulo);
        }

        [Fact]
        public void LosNivelesAltos_LleganAlRangoAlfa()
        {
            Assert.Equal("Cachorro", CalculadorNivel.TituloPara(1));
            Assert.Equal("Cazador", CalculadorNivel.TituloPara(8));
            Assert.Equal("Lobo Alfa", CalculadorNivel.TituloPara(20));
        }
    }
}
