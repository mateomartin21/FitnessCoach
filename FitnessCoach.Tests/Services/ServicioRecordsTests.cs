using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    public class ServicioRecordsTests
    {
        private const string IdentityAna = "identity-ana";
        private const string IdentityBruno = "identity-bruno";
        private const string Sentadilla = "quads-barbell-full-squat";

        private static (ServicioRecords servicio, UsuarioPerfil ana) Montar()
        {
            var ana = new UsuarioPerfil
            {
                IdentityUserId = IdentityAna,
                Nombre = "Ana",
                Edad = 30,
                EstaturaCm = 165,
                PesoKg = 62
            };

            var perfiles = new ServicioPerfilUsuario(new RepositorioUsuarioFalso(ana));
            return (new ServicioRecords(perfiles), ana);
        }

        [Fact]
        public void PrimeraMarca_SiempreEsRecord()
        {
            var (servicio, ana) = Montar();

            var resultado = servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 80, 5);

            Assert.True(resultado.EsNuevoRecord);
            Assert.Null(resultado.MejoraKg);   // no hay anterior con qué comparar
            Assert.Single(ana.RecordsPersonales);
        }

        [Fact]
        public void MasPeso_SuperaElRecordAnterior()
        {
            var (servicio, ana) = Montar();
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 80, 5);

            var resultado = servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 90, 3);

            Assert.True(resultado.EsNuevoRecord);
            Assert.Equal(10, resultado.MejoraKg);
            Assert.Equal(90, ana.RecordsPersonales[0].PesoKg);
        }

        [Fact]
        public void MenosPeso_NoPisaElRecord()
        {
            var (servicio, ana) = Montar();
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 5);

            var resultado = servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 80, 12);

            Assert.False(resultado.EsNuevoRecord);
            Assert.Equal(100, ana.RecordsPersonales[0].PesoKg);   // el récord sobrevive
        }

        [Fact]
        public void MismoPesoYMasRepeticiones_EsRecord()
        {
            var (servicio, ana) = Montar();
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 5);

            var resultado = servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 8);

            Assert.True(resultado.EsNuevoRecord);
            Assert.Equal(0, resultado.MejoraKg);   // mismo peso: la mejora está en las reps
            Assert.Equal(8, ana.RecordsPersonales[0].Repeticiones);
        }

        [Fact]
        public void MismoPesoYMenosRepeticiones_NoEsRecord()
        {
            var (servicio, _) = Montar();
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 8);

            var resultado = servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 5);

            Assert.False(resultado.EsNuevoRecord);
        }

        [Fact]
        public void CadaEjercicioTieneSuPropioRecord()
        {
            var (servicio, ana) = Montar();

            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 5);
            servicio.Registrar(IdentityAna, "pectorals-barbell-bench-press", "Press de banca", 70, 8);

            Assert.Equal(2, ana.RecordsPersonales.Count);
        }

        [Fact]
        public void UnEjercicioNuncaTieneDosRecords()
        {
            var (servicio, ana) = Montar();

            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 80, 5);
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 90, 5);
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 5);

            // Se actualiza la marca vigente, no se acumulan filas.
            Assert.Single(ana.RecordsPersonales);
            Assert.Equal(100, ana.RecordsPersonales[0].PesoKg);
        }

        [Fact]
        public void ElVolumenComparaSeriesDistintas()
        {
            var record = new RecordPersonal { PesoKg = 100, Repeticiones = 5 };

            Assert.Equal(500, record.Volumen);
        }

        [Fact]
        public void Registrar_SinEjercicio_Lanza()
        {
            var (servicio, _) = Montar();

            Assert.Throws<ArgumentException>(
                () => servicio.Registrar(IdentityAna, "", "Sentadilla", 80, 5));
        }

        [Fact]
        public void ObtenerDeEjercicio_DevuelveLaMarcaVigente()
        {
            var (servicio, _) = Montar();
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 95, 4);

            var record = servicio.ObtenerDeEjercicio(IdentityAna, Sentadilla);

            Assert.NotNull(record);
            Assert.Equal(95, record!.PesoKg);
        }

        // --- Aislamiento entre cuentas ---

        [Fact]
        public void LosRecordsDeOtraCuenta_NoSeVen()
        {
            var (servicio, _) = Montar();
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 5);

            Assert.Empty(servicio.ObtenerTodos(IdentityBruno));
            Assert.Null(servicio.ObtenerDeEjercicio(IdentityBruno, Sentadilla));
        }

        [Fact]
        public void Eliminar_UnRecordDeOtraCuenta_DevuelveFalse()
        {
            var (servicio, ana) = Montar();
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 5);
            var idDeAna = ana.RecordsPersonales[0].Id;

            Assert.False(servicio.Eliminar(IdentityBruno, idDeAna));
            Assert.Single(ana.RecordsPersonales);
        }

        [Fact]
        public void ElRecordDeUnUsuario_NoAfectaAlDeOtro()
        {
            // Bruno registra una marca menor: la de Ana no se toca.
            var (servicio, ana) = Montar();
            servicio.Registrar(IdentityAna, Sentadilla, "Sentadilla", 100, 5);

            servicio.Registrar(IdentityBruno, Sentadilla, "Sentadilla", 40, 5);

            Assert.Equal(100, ana.RecordsPersonales[0].PesoKg);
        }
    }
}
