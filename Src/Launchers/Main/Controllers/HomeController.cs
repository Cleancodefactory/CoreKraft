﻿using Ccf.Ck.Launchers.Main.Utils;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Ccf.Ck.Launchers.Main.Controllers
{
    public class HomeController : Controller
    {
        private KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        public HomeController(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        public IActionResult Index(string theme)
        {
            if (theme != null && _KraftGlobalConfigurationSettings.GeneralSettings.EnableThemeChange)
            {
                _KraftGlobalConfigurationSettings.GeneralSettings.Theme = theme;
                Styles styles = BundleCollection.Instance.Profile(_KraftGlobalConfigurationSettings.GeneralSettings.DefaultStartModule).Styles;
                styles.RemoveAllBundles();
            }
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

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            CookieHandler.AppendCookie(Response, culture);
            return LocalRedirect(returnUrl);
        }

        [Route("/{**catchAll}")]
        public IActionResult CatchAll(string catchAll)
        {
            return View("Index", _KraftGlobalConfigurationSettings);
        }
    }
}
