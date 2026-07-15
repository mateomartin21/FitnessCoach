using FitnessCoach.Domain.Ports;
using FitnessCoach.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCoach.Controllers
{
    public class AlimentacionController : Controller
    {
        private readonly IRepositorioUsuario _repositorio;
        private readonly IGeneradorAlimentacion _generador;

        public AlimentacionController(IRepositorioUsuario repositorio, IGeneradorAlimentacion generador)
        {
            _repositorio = repositorio;
            _generador = generador;
        }

        public IActionResult Index()
        {
            var usuario = _repositorio.ObtenerPorId(1);
            if (usuario == null || usuario.ObjetivoActual == null)
                return RedirectToAction("Index", "Perfil");

            var plan = _generador.GenerarPlanParaObjetivo(usuario.ObjetivoActual);
            return View(plan);
        }
    }
}
