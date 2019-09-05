using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Ccf.Ck.Launchers.Main.Controllers
{
    public class HomeController : Controller
    {
        KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;

        public HomeController(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }
        public IActionResult Index()
        {
            return View(_KraftGlobalConfigurationSettings);
        }
    }
}
