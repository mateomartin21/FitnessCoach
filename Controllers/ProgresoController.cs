using FitnessCoach.Domain.Models;
using FitnessCoach.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCoach.Controllers
{
    public class ProgresoController : Controller
    {
        private readonly IRepositorioUsuario _repositorio;

        public ProgresoController(IRepositorioUsuario repositorio)
        {
            _repositorio = repositorio;
        }

        // 1. Mostrar la pantalla con el historial
        public IActionResult Index()
        {
            var usuario = _repositorio.ObtenerPorId(1);
            if (usuario == null)
            {
                return RedirectToAction("Index", "Perfil");
            }

            // Ordenamos la lista para que el registro más nuevo salga arriba
            var historial = usuario.HistorialProgreso.OrderByDescending(r => r.Fecha).ToList();

            // Pasamos el peso actual a la vista usando ViewBag
            ViewBag.PesoActual = usuario.PesoKg;

            return View(historial);
        }

        // 2. Recibir el formulario, guardar y recargar la página (Server-side rendering)
        [HttpPost]
        public IActionResult RegistrarPeso(double NuevoPeso, string Notas)
        {
            var usuario = _repositorio.ObtenerPorId(1);

            if (usuario != null)
            {
                // Crear el nuevo punto en el historial
                var nuevoRegistro = new RegistroProgreso
                {
                    Fecha = DateTime.Now,
                    PesoKg = NuevoPeso,
                    Notas = Notas ?? ""
                };

                usuario.HistorialProgreso.Add(nuevoRegistro);

                // ¡Clave! Actualizamos el peso maestro del usuario para que sus calorías cambien
                usuario.PesoKg = NuevoPeso;

                // Guardar cambios en nuestro repositorio temporal
                _repositorio.Guardar(usuario);
            }

            // Redirigir a la misma pantalla para ver la tabla actualizada
            return RedirectToAction("Index");
        }
    }
}
