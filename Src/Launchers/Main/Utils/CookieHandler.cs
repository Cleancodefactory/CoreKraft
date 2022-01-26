using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccf.Ck.Launchers.Main.Utils
{
    public class CookieHandler
    {
        public static RequestCulture AppendCookie(HttpResponse response, string culture)
        {
            RequestCulture requestCulture = new RequestCulture(culture);
            response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
            response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(requestCulture),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true, IsEssential = true, SameSite=SameSiteMode.Strict });
            return requestCulture;
        }

        public static void RemoveCookie(HttpResponse response, string cookieName)
        {
            var cookies = response.Cookies;
            response.Cookies.Delete(cookieName, new CookieOptions()
            {
                Secure = true,
            });
        }
    }
}
