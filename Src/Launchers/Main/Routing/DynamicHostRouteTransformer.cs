using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ccf.Ck.Launchers.Main.Routing
{
    public class DynamicHostRouteTransformer : DynamicRouteValueTransformer
    {
        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            KraftGlobalConfigurationSettings kraftGlobalConfiguration = httpContext.RequestServices.GetService<KraftGlobalConfigurationSettings>();
            string[] fullAddress = httpContext.Request.Headers["Host"].ToString().Split('.');
            foreach (RouteMapping routing in kraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.RouteMappings)
            {
                if (IsHostMatch(fullAddress, routing.Host))
                {
                    Regex rg = new Regex(routing.SlugExpression, RegexOptions.IgnoreCase);
                    if (rg.Matches(httpContext.Request.Path).Count > 0)
                    {
                        values["controller"] = routing.Controller;
                        values["action"] = routing.Action;
                        break;
                    }
                }
            }
            return new ValueTask<RouteValueDictionary>(values);
        }

        private static bool IsHostMatch(string[] fullAddress, string configuredHost)
        {
            if (fullAddress.Length > 0)//has domain
            {
                if (fullAddress[0].Equals(configuredHost, StringComparison.OrdinalIgnoreCase)) //we are looking for the first segment
                {
                    return true;
                }
                return false;
            }
            else if (fullAddress.Length == 1) //no subdomains
            {
                return string.IsNullOrEmpty(configuredHost);
            }
            return false;
        }
    }
}
