using Ccf.Ck.Models.Settings;
using Ccf.Ck.Web.Middleware.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Web.Middleware
{
    internal class KraftToolsRouteBuilder
    {
        internal static void MakeRouters(IApplicationBuilder app, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            RouteHandler routesHandler;
            if (IsEnabled(kraftGlobalConfigurationSettings, "recorder"))//Recorder enabled from configuration
            {
                routesHandler = new RouteHandler(RecorderDelegate.ExecutionDelegate(app, kraftGlobalConfigurationSettings));

                RouteBuilder routesBuilderRecorder = new RouteBuilder(app, routesHandler);

                //we expect the routing to be like this:
                //domain.com/recorder/0|1|2?lang=de
                routesBuilderRecorder.MapRoute(
                    name: "recorder",
                    template: "tools/recorder/{p:int:range(0,3)}",
                    defaults: null,
                    constraints: null,
                    dataTokens: new { key = "recorder" }
                );
                app.UseRouter(routesBuilderRecorder.Build());
            }
            if (IsEnabled(kraftGlobalConfigurationSettings, "signals"))//Signals enabled from configuration
            {
                routesHandler = new RouteHandler(SignalDelegate.ExecutionDelegate(app, kraftGlobalConfigurationSettings));

                RouteBuilder routesBuilderRecorder = new RouteBuilder(app, routesHandler);

                //we expect the routing to be like this:
                //domain.com/signals/0|1|2?lang=de
                routesBuilderRecorder.MapRoute(
                    name: "signals",
                    template: "tools/signals",
                    defaults: null,
                    constraints: null,
                    dataTokens: new { key = "signals" }
                );
                app.UseRouter(routesBuilderRecorder.Build());
            }
            if (IsEnabled(kraftGlobalConfigurationSettings, "errors"))//Errors enabled from configuration
            {
            }
            if (IsEnabled(kraftGlobalConfigurationSettings, "profiler"))//Profiler enabled from configuration
            {
            }

            //if (true)//Errors enabled from configuration
            //if (true)//Profiler enabled from configuration

        }

        private static bool IsEnabled(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, string kind)
        {
            foreach (ToolSettings tool in kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings)
            {
                if (tool.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase))
                {
                    return tool.Enabled;
                }
            }
            return false;
        }

    }
}
