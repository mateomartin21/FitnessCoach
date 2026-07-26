using FitnessCoach.Domain.Models.Gamificacion;
using Xunit;

namespace FitnessCoach.Tests.Gamificacion
{
    public class CalculadorXPTests
    {
        [Fact]
        public void SinHechos_NoHayXp()
        {
            Assert.Equal(0, CalculadorXP.Base(EstadisticasUsuario.Vacias));
        }

        [Fact]
        public void CadaHechoSumaSuXp()
        {
            var e = new EstadisticasUsuario(
                TotalEntrenamientos: 2,   // 100
                TotalRecords: 1,          //  40
                DiasConDiario: 3,         //  30
                TotalRegistrosPeso: 2,    //  30
                RachaActual: 1,
                RachaMaxima: 4,           // 100 (bono de constancia)
                TieneObjetivo: true,
                EntrenamientosEstaSemana: 1,
                RegistrosPesoEstaSemana: 1,
                DiasConDiarioEstaSemana: 1);

            Assert.Equal(300, CalculadorXP.Base(e));
        }

        [Fact]
        public void LaConstanciaPesa_LaMejorRachaAportaBono()
        {
            var sinRacha = EstadisticasUsuario.Vacias with { TotalEntrenamientos = 1 };
            var conRacha = sinRacha with { RachaMaxima = 5 };

            var diferencia = CalculadorXP.Base(conRacha) - CalculadorXP.Base(sinRacha);

            Assert.Equal(5 * CalculadorXP.PorDiaDeMejorRacha, diferencia);
        }
    }
}
