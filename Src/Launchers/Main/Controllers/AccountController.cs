using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace Ccf.Ck.Launchers.Main.Controllers
{
    public class AccountController : Controller
    {
        private readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;

        public AccountController(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        [HttpGet]
        public ActionResult SignIn(string returnUrl)
        {
            // Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            // Note: the authenticationType parameter must match the value configured in Startup.cs
            if (string.IsNullOrEmpty(returnUrl))
            {
                string redirAfterLogin = _KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RedirectAfterLogin;
                returnUrl = Url.Action("Index", "Home");
                if (!string.IsNullOrWhiteSpace(redirAfterLogin) && !string.IsNullOrEmpty(returnUrl)) {
                    if (!returnUrl.EndsWith('/')) returnUrl += '/';
                    if (redirAfterLogin.StartsWith('/')) redirAfterLogin = redirAfterLogin.TrimStart('/');
                    returnUrl += redirAfterLogin;
                }
            }
            AuthenticationProperties authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = returnUrl
                
            };
            return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet, HttpPost]
        public new ActionResult SignOut()
        {
            // Instruct the cookies middleware to delete the local cookie created when the user agent
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}