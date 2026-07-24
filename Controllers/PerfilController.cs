using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models.Objetivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCoach.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly IServicioPerfilUsuario _perfiles;
        private readonly ICalculadorCalorico _calculador;

        public PerfilController(IServicioPerfilUsuario perfiles, ICalculadorCalorico calculador)
        {
            _perfiles = perfiles;
            _calculador = calculador;
        }

        private string IdentityId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            ViewBag.CaloriasRecomendadas = Math.Round(_calculador.CalcularCaloriasDiarias(usuario), 0);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarPerfil(string Nombre, int Edad, double PesoKg, double EstaturaCm, string TipoObjetivo)
        {
            ObjetivoFitness objetivo = TipoObjetivo switch
            {
                "Perder"  => new ObjetivoPerderPeso(),
                "Musculo" => new ObjetivoGanarMusculo(),
                _         => new ObjetivoRecomposicion()
            };

            // Traemos el perfil del usuario ACTUAL y lo actualizamos. Sin Id=1.
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            usuario.Nombre = Nombre;
            usuario.Edad = Edad;
            usuario.PesoKg = PesoKg;
            usuario.EstaturaCm = EstaturaCm;
            usuario.ObjetivoActual = objetivo;

            _perfiles.Guardar(usuario);
            return RedirectToAction("Index");
        }
    }
}
