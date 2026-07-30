using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    public class ServicioPerfilUsuarioTests
    {
        private const string IdentityAna = "identity-ana";
        private const string IdentityBruno = "identity-bruno";

        [Fact]
        public void ObtenerOCrear_UsuarioNuevo_CreaPerfilYLoAsociaASuIdentidad()
        {
            // Arrange — repositorio vacío: es la primera vez que Ana entra
            var repositorio = new RepositorioUsuarioFalso();
            var servicio = new ServicioPerfilUsuario(repositorio);

            // Act
            var perfil = servicio.ObtenerOCrear(IdentityAna);

            // Assert
            Assert.Equal(IdentityAna, perfil.IdentityUserId);
            Assert.Equal(1, repositorio.VecesQueSeGuardo);
            Assert.NotNull(repositorio.ObtenerPorIdentityUserId(IdentityAna));
        }

        [Fact]
        public void ObtenerOCrear_UsuarioNuevo_ArrancaConObjetivoRecomposicion()
        {
            // Arrange
            var servicio = new ServicioPerfilUsuario(new RepositorioUsuarioFalso());

            // Act
            var perfil = servicio.ObtenerOCrear(IdentityAna);

            // Assert — el perfil por defecto no queda sin objetivo
            Assert.IsType<ObjetivoRecomposicion>(perfil.ObjetivoActual);
        }

        [Fact]
        public void ObtenerOCrear_UsuarioExistente_DevuelveElSuyoYNoGuardaDeNuevo()
        {
            // Arrange
            var existente = new UsuarioPerfil { IdentityUserId = IdentityAna, Nombre = "Ana", PesoKg = 62 };
            var repositorio = new RepositorioUsuarioFalso(existente);
            var servicio = new ServicioPerfilUsuario(repositorio);

            // Act
            var perfil = servicio.ObtenerOCrear(IdentityAna);

            // Assert — mismo perfil, sin crear un duplicado ni tocar el repositorio
            Assert.Same(existente, perfil);
            Assert.Equal("Ana", perfil.Nombre);
            Assert.Equal(0, repositorio.VecesQueSeGuardo);
        }

        [Fact]
        public void ObtenerOCrear_ConVariosUsuarios_DevuelveElDeLaIdentidadPedida()
        {
            // Arrange — el escenario que motivó la Fase 2: dos cuentas en la misma base
            var repositorio = new RepositorioUsuarioFalso(
                new UsuarioPerfil { IdentityUserId = IdentityAna, Nombre = "Ana", PesoKg = 62 },
                new UsuarioPerfil { IdentityUserId = IdentityBruno, Nombre = "Bruno", PesoKg = 88 });
            var servicio = new ServicioPerfilUsuario(repositorio);

            // Act
            var perfilDeBruno = servicio.ObtenerOCrear(IdentityBruno);

            // Assert — nadie ve los datos del otro
            Assert.Equal("Bruno", perfilDeBruno.Nombre);
            Assert.Equal(88, perfilDeBruno.PesoKg);
        }

        [Fact]
        public void Obtener_UsuarioInexistente_DevuelveNullSinCrearNada()
        {
            // Arrange
            var repositorio = new RepositorioUsuarioFalso();
            var servicio = new ServicioPerfilUsuario(repositorio);

            // Act
            var perfil = servicio.Obtener(IdentityAna);

            // Assert — Obtener consulta, no crea (a diferencia de ObtenerOCrear)
            Assert.Null(perfil);
            Assert.Equal(0, repositorio.VecesQueSeGuardo);
        }

        [Fact]
        public void Obtener_UsuarioExistente_DevuelveSuPerfil()
        {
            // Arrange
            var repositorio = new RepositorioUsuarioFalso(
                new UsuarioPerfil { IdentityUserId = IdentityAna, Nombre = "Ana" });
            var servicio = new ServicioPerfilUsuario(repositorio);

            // Act
            var perfil = servicio.Obtener(IdentityAna);

            // Assert
            Assert.NotNull(perfil);
            Assert.Equal("Ana", perfil!.Nombre);
        }

        [Fact]
        public void ObtenerOCrear_VariasVecesEnLaMismaPeticion_LeeLaBaseUnaSolaVez()
        {
            // Progreso lo pide seis veces: el controlador y cada servicio. Contra SQL cada
            // lectura son cinco consultas (el perfil y sus cuatro colecciones).
            var repositorio = new RepositorioUsuarioFalso(
                new UsuarioPerfil { IdentityUserId = IdentityAna, Nombre = "Ana" });
            var servicio = new ServicioPerfilUsuario(repositorio);

            var primera = servicio.ObtenerOCrear(IdentityAna);
            for (int i = 0; i < 5; i++) servicio.ObtenerOCrear(IdentityAna);

            Assert.Equal(1, repositorio.VecesQueSeBuscoPorIdentidad);
            Assert.Same(primera, servicio.ObtenerOCrear(IdentityAna));
        }

        [Fact]
        public void ObtenerOCrear_ConDosIdentidades_NoConfundeLosPerfiles()
        {
            // La clave es la identidad: recordar no debe mezclar cuentas.
            var repositorio = new RepositorioUsuarioFalso(
                new UsuarioPerfil { IdentityUserId = IdentityAna, Nombre = "Ana" },
                new UsuarioPerfil { IdentityUserId = IdentityBruno, Nombre = "Bruno" });
            var servicio = new ServicioPerfilUsuario(repositorio);

            Assert.Equal("Ana", servicio.ObtenerOCrear(IdentityAna).Nombre);
            Assert.Equal("Bruno", servicio.ObtenerOCrear(IdentityBruno).Nombre);
            Assert.Equal("Ana", servicio.ObtenerOCrear(IdentityAna).Nombre);
            Assert.Equal(2, repositorio.VecesQueSeBuscoPorIdentidad);
        }

        [Fact]
        public void ObtenerOCrear_TrasCrearUnPerfilNuevo_NoVuelveALaBase()
        {
            var repositorio = new RepositorioUsuarioFalso();
            var servicio = new ServicioPerfilUsuario(repositorio);

            var creado = servicio.ObtenerOCrear(IdentityAna);

            Assert.Same(creado, servicio.ObtenerOCrear(IdentityAna));
            Assert.Equal(1, repositorio.VecesQueSeGuardo);   // no lo dio de alta dos veces
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ObtenerOCrear_SinIdentityUserId_LanzaArgumentException(string? identityUserId)
        {
            // Arrange
            var servicio = new ServicioPerfilUsuario(new RepositorioUsuarioFalso());

            // Act + Assert — sin identidad no hay dueño posible: falla fuerte en vez de inventar un perfil
            Assert.Throws<ArgumentException>(() => servicio.ObtenerOCrear(identityUserId!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Obtener_SinIdentityUserId_LanzaArgumentException(string? identityUserId)
        {
            // Arrange
            var servicio = new ServicioPerfilUsuario(new RepositorioUsuarioFalso());

            // Act + Assert
            Assert.Throws<ArgumentException>(() => servicio.Obtener(identityUserId!));
        }

        [Fact]
        public void Guardar_DelegaEnElRepositorio()
        {
            // Arrange
            var repositorio = new RepositorioUsuarioFalso();
            var servicio = new ServicioPerfilUsuario(repositorio);
            var perfil = servicio.ObtenerOCrear(IdentityAna);

            // Act — el usuario edita su perfil desde la web
            perfil.Nombre = "Ana Actualizada";
            perfil.PesoKg = 60;
            servicio.Guardar(perfil);

            // Assert — el cambio llegó al repositorio (1 guardado del alta + 1 de la edición)
            Assert.Equal(2, repositorio.VecesQueSeGuardo);
            Assert.Equal("Ana Actualizada", repositorio.ObtenerPorIdentityUserId(IdentityAna)!.Nombre);
        }
    }
}
