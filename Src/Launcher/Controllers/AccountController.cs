using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Ccf.Ck.Libs.Logging;

namespace Ccf.Ck.Launcher.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult SignIn()
        {
            // Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            // Note: the authenticationType parameter must match the value configured in Startup.cs
            KraftLogger.LogDebug("ActionResult SignIn");
            AuthenticationProperties authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            };
            return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet, HttpPost]
        public ActionResult SignOut()
        {
            // Instruct the cookies middleware to delete the local cookie created when the user agent
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet, HttpPost]
        public ActionResult Claims()
        {
            return View();
        }
    }
}