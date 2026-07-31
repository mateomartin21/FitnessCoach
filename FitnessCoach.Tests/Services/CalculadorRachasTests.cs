using FitnessCoach.Application.Services;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    public class CalculadorRachasTests
    {
        // "Hoy" fijo: la racha no puede depender de cuándo se corran las pruebas.
        private static readonly DateOnly Hoy = new(2026, 7, 24);

        private static DateTime Dia(int diasAtras) =>
            Hoy.AddDays(-diasAtras).ToDateTime(new TimeOnly(18, 0));

        [Fact]
        public void SinEntrenamientos_AmbasRachasEnCero()
        {
            var rachas = CalculadorRachas.Calcular(Array.Empty<DateTime>(), Hoy);

            Assert.Equal(0, rachas.Actual);
            Assert.Equal(0, rachas.MasLarga);
        }

        [Fact]
        public void EntrenandoHoy_LaRachaActualEsUno()
        {
            var rachas = CalculadorRachas.Calcular(new[] { Dia(0) }, Hoy);

            Assert.Equal(1, rachas.Actual);
            Assert.Equal(1, rachas.MasLarga);
        }

        [Fact]
        public void TresDiasSeguidosHastaHoy_RachaActualDeTres()
        {
            var rachas = CalculadorRachas.Calcular(new[] { Dia(2), Dia(1), Dia(0) }, Hoy);

            Assert.Equal(3, rachas.Actual);
            Assert.Equal(3, rachas.MasLarga);
        }

        [Fact]
        public void EntrenoAyerPeroNoHoy_LaRachaSigueViva()
        {
            // Si la racha se cortara a la medianoche, quien entrena de tarde la vería
            // en cero cada mañana antes de entrenar.
            var rachas = CalculadorRachas.Calcular(new[] { Dia(2), Dia(1) }, Hoy);

            Assert.Equal(2, rachas.Actual);
        }

        [Fact]
        public void UltimoEntrenamientoHaceDosDias_LaRachaActualSeCorta()
        {
            var rachas = CalculadorRachas.Calcular(new[] { Dia(3), Dia(2) }, Hoy);

            Assert.Equal(0, rachas.Actual);
            Assert.Equal(2, rachas.MasLarga);   // pero el récord se conserva
        }

        [Fact]
        public void VariosEntrenamientosElMismoDia_CuentanComoUnSoloDia()
        {
            var mismoDiaTemprano = Hoy.ToDateTime(new TimeOnly(7, 0));
            var mismoDiaTarde = Hoy.ToDateTime(new TimeOnly(20, 0));

            var rachas = CalculadorRachas.Calcular(new[] { mismoDiaTemprano, mismoDiaTarde }, Hoy);

            Assert.Equal(1, rachas.Actual);
            Assert.Equal(1, rachas.MasLarga);
        }

        [Fact]
        public void ConUnHuecoEnElMedio_LaMasLargaEsElMejorTramo()
        {
            // Cinco días seguidos hace tiempo, hueco, y dos días recientes.
            var fechas = new[] { Dia(20), Dia(19), Dia(18), Dia(17), Dia(16), Dia(1), Dia(0) };

            var rachas = CalculadorRachas.Calcular(fechas, Hoy);

            Assert.Equal(2, rachas.Actual);
            Assert.Equal(5, rachas.MasLarga);
        }

        [Fact]
        public void LasFechasDesordenadas_SeOrdenanAntesDeContar()
        {
            var fechas = new[] { Dia(0), Dia(2), Dia(1) };

            var rachas = CalculadorRachas.Calcular(fechas, Hoy);

            Assert.Equal(3, rachas.Actual);
        }

        [Fact]
        public void EntrenamientosSoloEnElPasadoLejano_RachaActualEnCero()
        {
            var rachas = CalculadorRachas.Calcular(new[] { Dia(100), Dia(99) }, Hoy);

            Assert.Equal(0, rachas.Actual);
            Assert.Equal(2, rachas.MasLarga);
        }
    }
}
