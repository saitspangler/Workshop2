using Microsoft.AspNetCore.Mvc;

namespace TravelExperts.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}