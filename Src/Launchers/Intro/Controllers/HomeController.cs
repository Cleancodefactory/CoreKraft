using Microsoft.AspNetCore.Mvc;

namespace Ccf.Ck.Launchers.Intro.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}