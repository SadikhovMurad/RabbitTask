using Microsoft.AspNetCore.Mvc;

namespace RabbitTask.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
