using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ccf.Ck.Web.Middleware
{
    public class OptionsMiddleware
    {
        private readonly RequestDelegate _Next;
        private readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;

        public OptionsMiddleware(RequestDelegate next, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _Next = next;
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        public Task Invoke(HttpContext context)
        {
            return BeginInvoke(context);
        }

        private Task BeginInvoke(HttpContext context)
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = new[] { _KraftGlobalConfigurationSettings.GeneralSettings.CorsAllowedOrigins.GetAllowedOrigins(context.Request) };

            AllowMethod allowMethod = _KraftGlobalConfigurationSettings.GeneralSettings.CorsAllowedOrigins.GetAllowMethod(context.Request.Method);
            if (allowMethod != null)
            {
                context.Response.Headers["Access-Control-Allow-Headers"] = new[] { allowMethod.AllowHeaders };
                context.Response.Headers["Access-Control-Allow-Methods"] = new[] { _KraftGlobalConfigurationSettings.GeneralSettings.CorsAllowedOrigins.GetAllowMethods() };
                context.Response.Headers["Access-Control-Allow-Credentials"] = new[] { allowMethod.AllowCredentials.ToString().ToLower() };

                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 200;
                    return context.Response.WriteAsync("OK");
                }
            }

            return _Next.Invoke(context);
        }
    }

    public static class OptionsMiddlewareExtensions
    {
        public static IApplicationBuilder UseOptions(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OptionsMiddleware>();
        }
    }
}
