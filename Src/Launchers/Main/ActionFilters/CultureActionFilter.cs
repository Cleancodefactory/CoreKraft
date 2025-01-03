using Ccf.Ck.Launchers.Main.Utils;
using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ccf.Ck.Launchers.Main.ActionFilters
{
    public class CultureActionFilter : IActionFilter
    {
        private readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        public CultureActionFilter(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing because it is a part of the interface.
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            RequestCulture requestCulture = null;
            KeyValuePair<string, string>? langCookie = context.HttpContext.Request.Cookies?.FirstOrDefault(context => context.Key.Equals(CookieRequestCultureProvider.DefaultCookieName, StringComparison.OrdinalIgnoreCase));
            if (langCookie.HasValue)
            {
                if (langCookie.Value.Key == null)
                {
                    requestCulture = AppendCookie(context);
                }
                else
                {
                    ProviderCultureResult providerCultureResult = CookieRequestCultureProvider.ParseCookieValue(langCookie.Value.Value);
                    if (providerCultureResult != null)
                    {
                        try
                        {
                            string[] userLanguages;
                            string[] firstPartOfLang = providerCultureResult.Cultures[0].ToString().Split("-");
                            userLanguages = new string[] { providerCultureResult.Cultures[0].ToString(), firstPartOfLang[0] };
                            string preferencedLang = GetPreferencedLanguagesOrDefault(userLanguages);
                            if (!string.IsNullOrEmpty(userLanguages.FirstOrDefault(l => l.Equals(preferencedLang, StringComparison.OrdinalIgnoreCase))))
                            {
                                requestCulture = new RequestCulture(providerCultureResult.Cultures[0].ToString(), providerCultureResult.UICultures[0].ToString());
                            }
                            else
                            {
                                requestCulture = AppendCookie(context);
                            }
                        }
                        catch
                        {
                            requestCulture = AppendCookie(context);
                        }
                    }
                    else
                    {
                        requestCulture = AppendCookie(context);
                    }                    
                }
            }
            else
            {
                requestCulture = AppendCookie(context);

            }
            Thread.CurrentThread.CurrentCulture = requestCulture.Culture;
            Thread.CurrentThread.CurrentUICulture = requestCulture.UICulture;
        }

        private RequestCulture AppendCookie(ActionExecutingContext context)
        {
            string[] userLanguages = context.HttpContext.Request.GetTypedHeaders()
                       .AcceptLanguage
                       ?.OrderByDescending(x => x.Quality ?? 1) // Quality defines priority from 0 to 1, where 1 is the highest.
                       .Select(x => x.Value.ToString())
                       .ToArray() ?? Array.Empty<string>();
            string culture = GetPreferencedLanguagesOrDefault(userLanguages);
            return CookieHandler.AppendCookie(context.HttpContext.Response, culture, _KraftGlobalConfigurationSettings);
        }

        private string GetPreferencedLanguagesOrDefault(string[] userLanguages)
        {
            List<string> defaultLanguagesOrdered = _KraftGlobalConfigurationSettings.GeneralSettings.SupportedLanguages;

            //Absolute match
            foreach (string defaultLanguage in defaultLanguagesOrdered) 
            {
                foreach (string userLanguage in userLanguages)
                {
                    if (userLanguage.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        return defaultLanguage;
                    }
                }
            }

            //Relaxed match
            foreach (string defaultLanguage in defaultLanguagesOrdered) 
            {
                string[] partsUserLanguage = defaultLanguage.Split("-");
                foreach (string userLanguage in userLanguages)
                {
                    string[] partsDefaultLanguage = userLanguage.Split("-");
                    
                    if (partsUserLanguage[0].Equals(partsDefaultLanguage[0], StringComparison.OrdinalIgnoreCase))
                    {
                        return defaultLanguage;
                    }
                    else if(partsUserLanguage.Length == 2 && partsDefaultLanguage.Length == 2)
                    {
                        if (partsUserLanguage[1].Equals(partsDefaultLanguage[1], StringComparison.OrdinalIgnoreCase))
                        {
                            return defaultLanguage;
                        }
                    }
                }
            }

            //Defaults
            if (defaultLanguagesOrdered.Count == 0)
            {
                return "en";
            }
            else
            {
                return defaultLanguagesOrdered.Last();
            }
        }
    }
}
