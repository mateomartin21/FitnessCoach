using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    [Authorize]
    public class AlimentacionController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly IGeneradorAlimentacion _generador;
        private readonly IRepositorioAlimentos _catalogo;

        public AlimentacionController(
            IServicioPerfilUsuario perfiles,
            IGeneradorAlimentacion generador,
            IRepositorioAlimentos catalogo)
        {
            _perfiles = perfiles;
            _generador = generador;
            _catalogo = catalogo;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var usuario = _perfiles.Obtener(IdentityId);
            if (usuario == null || usuario.ObjetivoActual == null)
                return RedirectToAction("Index", "Perfil");

            var plan = _generador.GenerarPlanPara(usuario);
            return View(plan);
        }

        public IActionResult Preferencias()
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            var modelo = new PreferenciasViewModel
            {
                Dietas = new List<string>(usuario.Preferencias.DietasSeguidas),
                AlimentosExcluidos = new List<string>(usuario.Preferencias.AlimentosExcluidos)
            };

            return View(ConCatalogo(modelo));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarPreferencias(PreferenciasViewModel modelo)
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);

            // Solo se aceptan valores que existen: las dietas conocidas y los slugs del
            // catálogo. Así un POST manipulado no puede meter basura en las preferencias.
            var dietasValidas = PreferenciasViewModel.DietasDisponibles.Select(d => d.Valor).ToHashSet();
            var slugsValidos = _catalogo.ObtenerTodos().Select(a => a.Slug).ToHashSet();

            usuario.Preferencias.DietasSeguidas = (modelo.Dietas ?? new())
                .Where(dietasValidas.Contains).Distinct().ToList();
            usuario.Preferencias.AlimentosExcluidos = (modelo.AlimentosExcluidos ?? new())
                .Where(slugsValidos.Contains).Distinct().ToList();

            _perfiles.Guardar(usuario);

            TempData["PreferenciasGuardadas"] = true;
            return RedirectToAction("Index");
        }

        /// <summary>Adjunta el catálogo agrupado para dibujar las opciones de exclusión.</summary>
        private PreferenciasViewModel ConCatalogo(PreferenciasViewModel modelo)
        {
            modelo.CatalogoPorCategoria = _catalogo.ObtenerTodos()
                .GroupBy(a => a.Categoria)
                .OrderBy(g => g.Key)
                .ToList();

            return modelo;
        }
    }
}
