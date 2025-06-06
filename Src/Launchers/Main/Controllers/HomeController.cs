﻿using Ccf.Ck.Launchers.Main.Utils;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Launchers.Main.Controllers
{
    public class HomeController : Controller
    {
        private readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        private readonly Regex PATTERNSTATICFILES = new Regex(@"([\/]+.*\.[a-zA-Z]+)", RegexOptions.Compiled | RegexOptions.Singleline);

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
            
            //if (!User.Identity.IsAuthenticated)
            //{
            //    if (ControllerContext.HttpContext.Request.QueryString.HasValue)
            //    {
            //        var request = ControllerContext.HttpContext.Request;
            //        string absoluteUri = string.Concat(
            //            request.Scheme,
            //            "://",
            //            request.Host.ToUriComponent(),
            //            request.PathBase.ToUriComponent(),
            //            request.Path.ToUriComponent(),
            //            ControllerContext.HttpContext.Request.QueryString);
            //        //this.HttpContext.Session.SetString("returnurl", absoluteUri);
            //    }
            //}
            //else
            //{
            //    //OnTokenValidated = context => is where the returnurl session property is populated from the Authorization server
            //    //string returnUrl = this.HttpContext.Session.GetString("returnurl");
            //    //if (!string.IsNullOrEmpty(returnUrl))
            //    //{
            //    //    //Setting the value back to nothing after the first redirect
            //    //    this.HttpContext.Session.SetString("returnurl", string.Empty);
            //    //    return Redirect(returnUrl);
            //    //}
            //}

            return View(_KraftGlobalConfigurationSettings);
        }

        //We shouldn't redirect to not change the url
        public IActionResult HistoryNav()
        {
            return View("Index", _KraftGlobalConfigurationSettings);
        }

        public IActionResult Unsupported()
        {
            KraftLogger.LogInformation($"Method: public IActionResult Unsupported Browser");
            return View();
        }

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = "/";
                }
                CookieHandler.AppendCookie(Response, culture, _KraftGlobalConfigurationSettings);
            }
            catch { }
            returnUrl = Regex.Replace(returnUrl, @"[^\u0020-\u007E]", string.Empty);//remove all non ascii characters
            return LocalRedirect(returnUrl);
        }

        public IActionResult CatchAll(string catchAll)
        {
            RazorCatchAll razorCatchAll = _KraftGlobalConfigurationSettings.GeneralSettings.RazorAreaAssembly.ParseRazorCatchAll();
            if (razorCatchAll != null)
            {
                string param = razorCatchAll.CatchAllParameter;
                RouteValueDictionary v = new RouteValueDictionary();
                v.Add(param, catchAll);

                return RedirectToAction(razorCatchAll.CatchAllAction, razorCatchAll.CatchAllController, v);
            }

            if (!string.IsNullOrEmpty(catchAll))
            {
                if (PATTERNSTATICFILES.Matches(catchAll).Count > 0)
                {
                    KraftLogger.LogWarning($"Missing resource: {catchAll}");
                    return NoContent();
                }
            }
            return View("Index", _KraftGlobalConfigurationSettings);
        }

        public IActionResult Error()
        {
            IExceptionHandlerPathFeature exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            KraftLogger.LogCritical($"Method: public IActionResult Error for path: {exceptionHandlerPathFeature?.Path}", exceptionHandlerPathFeature?.Error);
            return View();
        }
    }
}
