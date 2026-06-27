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

        // Inyección de dependencias (DIP)
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

            double caloriasRecomendadas = _calculador.CalcularCaloriasDiarias(usuario);

            ViewBag.CaloriasRecomendadas = Math.Round(caloriasRecomendadas, 2);

            return View(usuario);
        }
        [HttpPost]
        public IActionResult GuardarPerfil(string Nombre, int Edad, double PesoKg, double EstaturaCm, string TipoObjetivo)
        {
            // 1. Instanciar el objetivo correcto aplicando polimorfismo (Principio Open/Closed)
            ObjetivoFitness objetivo = TipoObjetivo == "Perder"
                ? new ObjetivoPerderPeso()
                : new ObjetivoRecomposicion();

            // 2. Crear o actualizar el objeto del usuario
            var usuarioModificado = new UsuarioPerfil
            {
                Id = 1, // Usamos el ID fijo temporal de nuestra memoria
                Nombre = Nombre,
                Edad = Edad,
                PesoKg = PesoKg,
                EstaturaCm = EstaturaCm,
                ObjetivoActual = objetivo
            };

            // 3. Guardar en nuestro repositorio en memoria (Principio de Inversión de Dependencias)
            _repositorio.Guardar(usuarioModificado);

            // 4. Redirigir de nuevo a la pantalla principal del perfil para ver los nuevos cálculos actualizados
            return RedirectToAction("Index");
        }
    }
}
