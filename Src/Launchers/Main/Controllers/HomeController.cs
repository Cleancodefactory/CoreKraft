using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

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
            if (!User.Identity.IsAuthenticated)
            {
                if (ControllerContext.HttpContext.Request.QueryString.HasValue)
                {
                    var request = ControllerContext.HttpContext.Request;
                    string absoluteUri = string.Concat(
                        request.Scheme,
                        "://",
                        request.Host.ToUriComponent(),
                        request.PathBase.ToUriComponent(),
                        request.Path.ToUriComponent(),
                        ControllerContext.HttpContext.Request.QueryString);
                    this.HttpContext.Session.SetString("returnurl", absoluteUri);
                }
            }
            else
            {
                //OnTokenValidated = context => is where the returnurl session property is populated from the Authorization server
                string returnUrl = this.HttpContext.Session.GetString("returnurl");
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    //Setting the value back to nothing after the first redirect
                    this.HttpContext.Session.SetString("returnurl", string.Empty);
                    return Redirect(returnUrl);
                }
            }

            return View(_KraftGlobalConfigurationSettings);
        }
    }
}
