using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    public class ServicioProgresoTests
    {
        private const string IdentityAna = "identity-ana";
        private const string IdentityBruno = "identity-bruno";

        /// <summary>Arma el servicio sobre un repositorio falso y devuelve también el perfil de Ana.</summary>
        private static (ServicioProgreso servicio, UsuarioPerfil ana) Montar(params RegistroProgreso[] registros)
        {
            var ana = new UsuarioPerfil
            {
                IdentityUserId = IdentityAna,
                Nombre = "Ana",
                Edad = 30,
                EstaturaCm = 165,
                PesoKg = 62
            };

            int siguienteId = 1;
            foreach (var registro in registros)
            {
                if (registro.Id == 0) registro.Id = siguienteId++;
                ana.HistorialProgreso.Add(registro);
            }

            var repositorio = new RepositorioUsuarioFalso(ana);
            var perfiles = new ServicioPerfilUsuario(repositorio);
            return (new ServicioProgreso(perfiles), ana);
        }

        private static RegistroProgreso Registro(int diasAtras, double pesoKg) => new()
        {
            Fecha = DateTime.UtcNow.AddDays(-diasAtras),
            PesoKg = pesoKg
        };

        [Fact]
        public void Agregar_GuardaElRegistroYActualizaElPesoDelPerfil()
        {
            var (servicio, ana) = Montar();

            var registro = servicio.Agregar(IdentityAna, 60.5, "Primera pesada");

            Assert.Equal(60.5, registro.PesoKg);
            Assert.Equal(60.5, ana.PesoKg);
            Assert.Single(ana.HistorialProgreso);
        }

        [Fact]
        public void Agregar_PoneLaFechaEnUtc()
        {
            var (servicio, _) = Montar();

            var registro = servicio.Agregar(IdentityAna, 60, null);

            Assert.Equal(DateTimeKind.Utc, registro.Fecha.Kind);
        }

        [Fact]
        public void ObtenerHistorial_DevuelveDelMasRecienteAlMasAntiguo()
        {
            var (servicio, _) = Montar(
                Registro(diasAtras: 10, pesoKg: 65),
                Registro(diasAtras: 1, pesoKg: 62),
                Registro(diasAtras: 5, pesoKg: 63));

            var historial = servicio.ObtenerHistorial(IdentityAna);

            Assert.Equal(new[] { 62d, 63d, 65d }, historial.Select(r => r.PesoKg));
        }

        [Fact]
        public void Editar_CambiaLosDatosDelRegistro()
        {
            var (servicio, ana) = Montar(Registro(diasAtras: 3, pesoKg: 64));
            var id = ana.HistorialProgreso[0].Id;

            var resultado = servicio.Editar(IdentityAna, id, 63.2, "Corregido");

            Assert.True(resultado);
            Assert.Equal(63.2, ana.HistorialProgreso[0].PesoKg);
            Assert.Equal("Corregido", ana.HistorialProgreso[0].Notas);
        }

        [Fact]
        public void Editar_ElRegistroMasReciente_ArrastraElPesoDelPerfil()
        {
            var (servicio, ana) = Montar(
                Registro(diasAtras: 10, pesoKg: 65),
                Registro(diasAtras: 1, pesoKg: 62));   // el más reciente
            var idMasReciente = ana.HistorialProgreso[1].Id;

            servicio.Editar(IdentityAna, idMasReciente, 61, null);

            Assert.Equal(61, ana.PesoKg);
        }

        [Fact]
        public void Editar_UnRegistroViejo_NoTocaElPesoDelPerfil()
        {
            var (servicio, ana) = Montar(
                Registro(diasAtras: 10, pesoKg: 65),   // el viejo
                Registro(diasAtras: 1, pesoKg: 62));
            var idViejo = ana.HistorialProgreso[0].Id;

            servicio.Editar(IdentityAna, idViejo, 70, null);

            // El peso del perfil sigue al registro más reciente, que no cambió.
            Assert.Equal(62, ana.PesoKg);
        }

        [Fact]
        public void Eliminar_QuitaElRegistro()
        {
            var (servicio, ana) = Montar(Registro(diasAtras: 2, pesoKg: 64));
            var id = ana.HistorialProgreso[0].Id;

            var resultado = servicio.Eliminar(IdentityAna, id);

            Assert.True(resultado);
            Assert.Empty(ana.HistorialProgreso);
        }

        [Fact]
        public void Eliminar_ElMasReciente_DejaElPesoDelAnterior()
        {
            var (servicio, ana) = Montar(
                Registro(diasAtras: 10, pesoKg: 65),
                Registro(diasAtras: 1, pesoKg: 62));   // el más reciente
            var idMasReciente = ana.HistorialProgreso[1].Id;

            servicio.Eliminar(IdentityAna, idMasReciente);

            Assert.Equal(65, ana.PesoKg);
        }

        [Fact]
        public void Eliminar_ElUltimoQueQueda_ConservaElPesoAnterior()
        {
            // Poner el peso en 0 dejaría el perfil fuera de rango y rompería el cálculo calórico.
            var (servicio, ana) = Montar(Registro(diasAtras: 1, pesoKg: 62));
            var id = ana.HistorialProgreso[0].Id;

            servicio.Eliminar(IdentityAna, id);

            Assert.Empty(ana.HistorialProgreso);
            Assert.Equal(62, ana.PesoKg);
        }

        // --- Aislamiento entre cuentas ---

        [Fact]
        public void Editar_UnRegistroDeOtraCuenta_DevuelveFalse()
        {
            var (servicio, ana) = Montar(Registro(diasAtras: 1, pesoKg: 62));
            var idDeAna = ana.HistorialProgreso[0].Id;

            // Bruno intenta editar un registro de Ana usando su id.
            var resultado = servicio.Editar(IdentityBruno, idDeAna, 99, "hackeado");

            Assert.False(resultado);
            Assert.Equal(62, ana.HistorialProgreso[0].PesoKg);
        }

        [Fact]
        public void Eliminar_UnRegistroDeOtraCuenta_DevuelveFalse()
        {
            var (servicio, ana) = Montar(Registro(diasAtras: 1, pesoKg: 62));
            var idDeAna = ana.HistorialProgreso[0].Id;

            var resultado = servicio.Eliminar(IdentityBruno, idDeAna);

            Assert.False(resultado);
            Assert.Single(ana.HistorialProgreso);
        }

        [Fact]
        public void ObtenerRegistro_DeOtraCuenta_DevuelveNull()
        {
            var (servicio, ana) = Montar(Registro(diasAtras: 1, pesoKg: 62));
            var idDeAna = ana.HistorialProgreso[0].Id;

            Assert.Null(servicio.ObtenerRegistro(IdentityBruno, idDeAna));
        }
    }
}
