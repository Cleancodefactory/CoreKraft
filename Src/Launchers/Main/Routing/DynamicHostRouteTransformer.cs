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
            if (_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorizationAnyEndpoint 
                && _KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
            {
                if (!httpContext.User.Identity.IsAuthenticated)
                {
                    values["controller"] = "account";
                    values["action"] = "signin";
                    return new ValueTask<RouteValueDictionary>(values);
                }
            }
            foreach (RouteMapping routing in _KraftGlobalConfigurationSettings.GeneralSettings.RazorAreaAssembly.RouteMappings)
            {
                if (!string.IsNullOrEmpty(routing.SlugExpression))
                {
                    Regex rg = new Regex(routing.SlugExpression, RegexOptions.IgnoreCase);
                    Match match = rg.Match(httpContext.Request.Path);
                    if (match.Success)
                    {
                        values["controller"] = routing.Controller;
                        values["action"] = routing.Action;

                        foreach (Group group in match.Groups)
                        {
                            if (!string.IsNullOrWhiteSpace(group.Name) && group.Name.Length > 1)
                            {
                                values[group.Name.Trim()] = group.Value?.Trim();
                            }
                        }
                        break;
                    }
                }
            }
            return new ValueTask<RouteValueDictionary>(values);
        }
    }
}
