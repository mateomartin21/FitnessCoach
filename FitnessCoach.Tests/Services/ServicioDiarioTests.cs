using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Tests.Fakes;
using Xunit;

namespace FitnessCoach.Tests.Services
{
    public class ServicioDiarioTests
    {
        private static readonly DateOnly Hoy = new(2026, 7, 25);

        private static (ServicioDiario servicio, UsuarioPerfil usuario, RepositorioUsuarioFalso repo) Montar()
        {
            var usuario = new UsuarioPerfil
            {
                Id = 1, IdentityUserId = "u1", Nombre = "Ana",
                Edad = 30, EstaturaCm = 165, PesoKg = 65,
                ObjetivoActual = new ObjetivoPerderPeso()
            };
            var repo = new RepositorioUsuarioFalso(usuario);
            var servicio = new ServicioDiario(
                repo, RepositorioAlimentosFalso.ConCatalogoDePrueba(), new CalculadorCaloricoService());

            return (servicio, usuario, repo);
        }

        [Fact]
        public void Registrar_GuardaLosMacrosEscaladosDelAlimento()
        {
            var (servicio, usuario, _) = Montar();

            // 200 g de pechuga de pollo (22.5 g proteína/100 g) = 45 g de proteína.
            servicio.Registrar(usuario, "pechuga-de-pollo", 200, Hoy);

            var registro = Assert.Single(usuario.Diario);
            Assert.Equal("pechuga-de-pollo", registro.AlimentoSlug);
            Assert.Equal("Pechuga de pollo", registro.AlimentoNombre);
            Assert.Equal(45.0, registro.ProteinaG, precision: 2);
        }

        [Fact]
        public void Registrar_PersisteElPerfil()
        {
            var (servicio, usuario, repo) = Montar();

            servicio.Registrar(usuario, "pechuga-de-pollo", 150, Hoy);

            Assert.Equal(1, repo.VecesQueSeGuardo);
        }

        [Fact]
        public void Registrar_UnSlugInexistente_Lanza()
        {
            var (servicio, usuario, _) = Montar();

            Assert.Throws<ArgumentException>(
                () => servicio.Registrar(usuario, "dragon-a-la-parrilla", 150, Hoy));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Registrar_CantidadInvalida_Lanza(double gramos)
        {
            var (servicio, usuario, _) = Montar();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => servicio.Registrar(usuario, "pechuga-de-pollo", gramos, Hoy));
        }

        [Fact]
        public void Borrar_QuitaElRegistroDelUsuario()
        {
            var (servicio, usuario, _) = Montar();
            servicio.Registrar(usuario, "pechuga-de-pollo", 150, Hoy);
            var id = usuario.Diario.Single().Id = 7;   // simulamos el Id que asignaría la base

            servicio.Borrar(usuario, 7);

            Assert.Empty(usuario.Diario);
        }

        [Fact]
        public void Borrar_UnIdAjeno_NoTocaNada()
        {
            var (servicio, usuario, repo) = Montar();
            servicio.Registrar(usuario, "pechuga-de-pollo", 150, Hoy);
            var guardadosAntes = repo.VecesQueSeGuardo;

            servicio.Borrar(usuario, 999);   // no existe

            Assert.Single(usuario.Diario);
            Assert.Equal(guardadosAntes, repo.VecesQueSeGuardo);   // ni siquiera guardó
        }

        [Fact]
        public void ResumenDelDia_SumaSoloLosRegistrosDeEseDia()
        {
            var (servicio, usuario, _) = Montar();
            servicio.Registrar(usuario, "pechuga-de-pollo", 150, Hoy);
            servicio.Registrar(usuario, "arroz-integral", 150, Hoy);
            servicio.Registrar(usuario, "pechuga-de-pollo", 200, Hoy.AddDays(-1));   // otro día

            var resumen = servicio.ResumenDelDia(usuario, Hoy);

            Assert.Equal(2, resumen.Registros.Count);
            Assert.True(resumen.CaloriasConsumidas > 0);
            Assert.True(resumen.Objetivo.Calorias > 0);   // el objetivo se calculó desde el perfil
        }

        [Fact]
        public void ResumenDelDia_SinRegistros_DaVacioPeroConObjetivo()
        {
            var (servicio, usuario, _) = Montar();

            var resumen = servicio.ResumenDelDia(usuario, Hoy);

            Assert.True(resumen.SinRegistros);
            Assert.True(resumen.Objetivo.Calorias > 0);
        }

        [Fact]
        public void ResumenDelDia_ConPerfilSinDatosValidos_NoRompe()
        {
            // Un perfil recién creado con estatura 0 no permite calcular calorías;
            // el diario igual debe funcionar, solo que sin objetivo contra el cual comparar.
            var usuario = new UsuarioPerfil { Id = 2, IdentityUserId = "u2", ObjetivoActual = new ObjetivoPerderPeso() };
            var servicio = new ServicioDiario(
                new RepositorioUsuarioFalso(usuario),
                RepositorioAlimentosFalso.ConCatalogoDePrueba(),
                new CalculadorCaloricoService());

            var resumen = servicio.ResumenDelDia(usuario, Hoy);

            Assert.Equal(0, resumen.Objetivo.Calorias);
            Assert.Equal(0, resumen.PorcentajeCalorico);
        }

        [Fact]
        public void SinUsuario_Lanza()
        {
            var (servicio, _, _) = Montar();

            Assert.Throws<ArgumentNullException>(() => servicio.ResumenDelDia(null!, Hoy));
            Assert.Throws<ArgumentNullException>(() => servicio.Registrar(null!, "x", 100, Hoy));
        }
    }
}
