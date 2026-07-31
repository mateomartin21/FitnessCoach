using FitnessCoach.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FitnessCoach.Controllers
{
    public class HomeController : Controller
    {
        // Sin sesión no hay nada que hacer en la app: toda la navegación pide cuenta.
        // La bienvenida presenta a Koda y manda al login; la portada queda para quien ya entró.
        public IActionResult Index()
        {
            if (User.Identity is not { IsAuthenticated: true })
                return View("Bienvenida");

            return View();
        }

        // La portada, accesible sin cuenta desde la bienvenida.
        public IActionResult Conoce() => View("Index");

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
