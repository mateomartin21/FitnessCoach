using Microsoft.AspNetCore.Mvc;

namespace FitnessCoach.Controllers
{
    public class IaCoachController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}