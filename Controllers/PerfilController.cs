using FitnessCoach.Application.Services;
using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Models;
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
            ViewBag.ObjetivoNombre = usuario.ObjetivoActual?.Nombre ?? "No definido";
            ViewBag.CaloriasRecomendadas = Math.Round(_calculador.CalcularCaloriasDiarias(usuario), 0);
            return View(ADaptarAFormulario(usuario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarPerfil(PerfilViewModel modelo)
        {
            // Nada se toca hasta saber que los datos son válidos.
            if (!ModelState.IsValid)
            {
                ViewBag.ObjetivoNombre = NombreDelObjetivo(modelo.TipoObjetivo);
                ViewBag.CaloriasRecomendadas = null;   // con datos inválidos no hay cálculo que mostrar
                return View("Index", modelo);
            }

            // Traemos el perfil del usuario ACTUAL y lo actualizamos. Sin Id=1.
            var usuario = _perfiles.ObtenerOCrear(IdentityId);
            usuario.Nombre = modelo.Nombre;
            usuario.Edad = modelo.Edad;
            usuario.PesoKg = modelo.PesoKg;
            usuario.EstaturaCm = modelo.EstaturaCm;
            usuario.ObjetivoActual = CrearObjetivo(modelo.TipoObjetivo);

            _perfiles.Guardar(usuario);
            return RedirectToAction("Index");
        }

        private static PerfilViewModel ADaptarAFormulario(UsuarioPerfil usuario) => new()
        {
            Nombre = usuario.Nombre ?? string.Empty,
            Edad = usuario.Edad,
            PesoKg = usuario.PesoKg,
            EstaturaCm = usuario.EstaturaCm,
            TipoObjetivo = usuario.ObjetivoActual switch
            {
                ObjetivoPerderPeso => "Perder",
                ObjetivoGanarMusculo => "Musculo",
                _ => "Recomp"
            }
        };

        private static ObjetivoFitness CrearObjetivo(string tipo) => tipo switch
        {
            "Perder" => new ObjetivoPerderPeso(),
            "Musculo" => new ObjetivoGanarMusculo(),
            _ => new ObjetivoRecomposicion()
        };

        private static string NombreDelObjetivo(string tipo) => CrearObjetivo(tipo).Nombre;
    }
}
