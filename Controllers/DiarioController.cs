using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    [Authorize]
    public class DiarioController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly IServicioDiario _diario;
        private readonly IGeneradorAlimentacion _generador;
        private readonly IRepositorioAlimentos _catalogo;

        public DiarioController(
            IServicioPerfilUsuario perfiles,
            IServicioDiario diario,
            IGeneradorAlimentacion generador,
            IRepositorioAlimentos catalogo)
        {
            _perfiles = perfiles;
            _diario = diario;
            _generador = generador;
            _catalogo = catalogo;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(DateOnly? dia)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            if (usuario.ObjetivoActual is null)
                return RedirectToAction("Index", "Perfil");

            // Nunca se muestran días futuros: no se puede registrar comida que no pasó.
            var elDia = ClampHoy(dia, usuario);

            var modelo = new DiarioViewModel
            {
                Dia = elDia,
                Hoy = Hoy(usuario),
                Resumen = _diario.ResumenDelDia(usuario, elDia),
                Plan = _generador.GenerarPlanPara(usuario),
                CatalogoPorCategoria = _catalogo.ObtenerTodos()
                    .GroupBy(a => a.Categoria).OrderBy(g => g.Key).ToList()
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(string alimentoSlug, double gramos, DateOnly? dia)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            var elDia = ClampHoy(dia, usuario);

            // Rango razonable de una porción; fuera de eso es un error de tipeo.
            if (gramos is > 0 and <= 2000 && _catalogo.ObtenerPorSlug(alimentoSlug) is not null)
                _diario.Registrar(usuario, alimentoSlug, gramos, elDia);
            else
                TempData["ErrorDiario"] = "No se pudo registrar: revisa el alimento y la cantidad.";

            return RedirectToAction("Index", new { dia = elDia });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarComida(string nombreComida, DateOnly? dia)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            var elDia = ClampHoy(dia, usuario);

            // El plan es determinista, así que regenerarlo devuelve las mismas comidas
            // que vio el usuario. Se registra cada porción de la comida elegida.
            var plan = _generador.GenerarPlanPara(usuario);
            var comida = plan.Comidas.FirstOrDefault(c => c.NombreComida == nombreComida);

            if (comida is not null)
                foreach (var porcion in comida.Porciones)
                    _diario.Registrar(usuario, porcion.Alimento.Slug, porcion.Gramos, elDia);

            return RedirectToAction("Index", new { dia = elDia });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Borrar(int id, DateOnly? dia)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            _diario.Borrar(usuario, id);

            return RedirectToAction("Index", new { dia = ClampHoy(dia, usuario) });
        }

        /// <summary>
        /// El día pedido, nunca en el futuro; por defecto, hoy. "Hoy" es el del calendario
        /// del usuario, no el del servidor (D-25).
        /// </summary>
        private static DateOnly ClampHoy(DateOnly? dia, UsuarioPerfil usuario)
        {
            var hoy = Hoy(usuario);
            return dia is { } d && d <= hoy ? d : hoy;
        }

        private static DateOnly Hoy(UsuarioPerfil usuario) =>
            ZonaHorariaUsuario.Hoy(ZonaHorariaUsuario.De(usuario));
    }
}
