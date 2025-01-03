using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Launchers.Main.Utils
{
    public class CookieHandler
    {
        public static RequestCulture AppendCookie(HttpResponse response, string culture, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            string sanitizedCulture = SanitizeCulture(culture);

            if (string.IsNullOrEmpty(sanitizedCulture))
            {
                // Fallback to a default culture if input is invalid
                sanitizedCulture = kraftGlobalConfigurationSettings.GeneralSettings?.SupportedLanguages?.Last();
                if (string.IsNullOrEmpty(sanitizedCulture))
                {
                    // Fallback to a default culture if no supported languages are defined
                    sanitizedCulture = "en";
                }
            }

            RequestCulture requestCulture = new RequestCulture(sanitizedCulture);
            response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
            response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(requestCulture),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true, IsEssential = true, SameSite = SameSiteMode.Strict, HttpOnly = true });
            return requestCulture;
        }

        private static string SanitizeCulture(string culture)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return null;
            }

            // Define a regex pattern for valid culture strings
            const string culturePattern = @"^([a-z]{2,3})(-[A-Z]{2,3})?";
            Match match = Regex.Match(culture, culturePattern);

            if (match.Success)
            {
                // Extract the valid culture components
                return match.Value;
            }

            // Return null if the culture doesn't match the pattern
            return null;
        }

        public static void RemoveCookie(HttpResponse response, string cookieName)
        {
            var cookies = response.Cookies;
            response.Cookies.Delete(cookieName, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
            });
        }
    }
}
