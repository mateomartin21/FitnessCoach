using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Models.Objetivos;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCoach.Controllers
{
    public class PerfilController : Controller
    {
        private readonly IRepositorioUsuario _repositorio;
        private readonly ICalculadorCalorico _calculador;

        public PerfilController(IRepositorioUsuario repositorio, ICalculadorCalorico calculador)
        {
            _repositorio = repositorio;
            _calculador = calculador;
        }

        public IActionResult Index()
        {
            var usuario = _repositorio.ObtenerPorId(1);
            if (usuario == null)
            {
                usuario = new UsuarioPerfil
                {
                    Nombre = "Usuario Demo",
                    Edad = 25,
                    EstaturaCm = 170,
                    PesoKg = 70,
                    ObjetivoActual = new ObjetivoRecomposicion()
                };
                _repositorio.Guardar(usuario);
            }
            ViewBag.CaloriasRecomendadas = Math.Round(_calculador.CalcularCaloriasDiarias(usuario), 0);
            return View(usuario);
        }

        [HttpPost]
        public IActionResult GuardarPerfil(string Nombre, int Edad, double PesoKg, double EstaturaCm, string TipoObjetivo)
        {
            ObjetivoFitness objetivo = TipoObjetivo switch
            {
                "Perder"  => new ObjetivoPerderPeso(),
                "Musculo" => new ObjetivoGanarMusculo(),
                _         => new ObjetivoRecomposicion()
            };

            var usuario = new UsuarioPerfil
            {
                Id = 1,
                Nombre = Nombre,
                Edad = Edad,
                PesoKg = PesoKg,
                EstaturaCm = EstaturaCm,
                ObjetivoActual = objetivo
            };

            _repositorio.Guardar(usuario);
            return RedirectToAction("Index");
        }
    }
}
