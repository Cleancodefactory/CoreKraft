using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ccf.Ck.Launcher.Example.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _Logger;
        private readonly IHostingEnvironment _HostingEnvironment;

        public HomeController(ILogger<HomeController> logger, IHostingEnvironment hostingEnvironment)
        {
            _Logger = logger;
            _HostingEnvironment = hostingEnvironment;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}