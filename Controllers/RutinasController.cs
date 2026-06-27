using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Ports;
using FitnessCoach.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCoach.Controllers
{
    public class RutinasController : Controller
    {
        private readonly IRepositorioUsuario _repositorio;
        private readonly IGeneradorRutinas _generador;

        // Inyección del repositorio de datos y del servicio de rutinas
        public RutinasController(IRepositorioUsuario repositorio, IGeneradorRutinas generador)
        {
            _repositorio = repositorio;
            _generador = generador;
        }

        public IActionResult Index()
        {
            // 1. Obtenemos al usuario activo (seguimos usando el Id 1 de nuestra memoria)
            var usuario = _repositorio.ObtenerPorId(1);

            // Si el usuario no existe o no ha configurado un objetivo, lo mandamos al Perfil
            if (usuario == null || usuario.ObjetivoActual == null)
            {
                return RedirectToAction("Index", "Perfil");
            }

            // 2. Usamos el servicio para generar la rutina basada en su objetivo
            var rutinaGenerada = _generador.GenerarRutinaParaObjetivo(usuario.ObjetivoActual);

            // 3. Pasamos el modelo de la rutina a la vista
            return View(rutinaGenerada);
        }
    }
}
