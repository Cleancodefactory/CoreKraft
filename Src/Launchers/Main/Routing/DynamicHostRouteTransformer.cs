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
        KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        public DynamicHostRouteTransformer(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            foreach (RouteMapping routing in _KraftGlobalConfigurationSettings.GeneralSettings.RazorAreaAssembly.RouteMappings)
            {
                Regex rg = new Regex(routing.SlugExpression, RegexOptions.IgnoreCase);
                if (rg.Matches(httpContext.Request.Path).Count > 0)
                {
                    values["controller"] = routing.Controller;
                    values["action"] = routing.Action;
                    break;
                }
            }
            return new ValueTask<RouteValueDictionary>(values);
        }
    }
}
