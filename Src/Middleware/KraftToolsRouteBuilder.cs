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
            ToolSettings tool = GetTool(kraftGlobalConfigurationSettings, "recorder");
            if (tool != null && tool.Enabled)//Recorder enabled from configuration
            {
                routesHandler = new RouteHandler(RecorderDelegate.ExecutionDelegate(app, kraftGlobalConfigurationSettings));

                RouteBuilder routesBuilderRecorder = new RouteBuilder(app, routesHandler);

                //we expect the routing to be like this:
                //domain.com/recorder/0|1|2?lang=de
                routesBuilderRecorder.MapRoute(
                    name: "recorder",
                    template: tool.Url,
                    defaults: null,
                    constraints: null,
                    dataTokens: new { key = "recorder" }
                );
                app.UseRouter(routesBuilderRecorder.Build());
            }
            tool = GetTool(kraftGlobalConfigurationSettings, "signals");
            if (tool != null && tool.Enabled)//Signals enabled from configuration
            {
                routesHandler = new RouteHandler(SignalDelegate.ExecutionDelegate(app, kraftGlobalConfigurationSettings));

                RouteBuilder routesBuilderRecorder = new RouteBuilder(app, routesHandler);

                //we expect the routing to be like this:
                //domain.com/signals/0|1|2?lang=de
                routesBuilderRecorder.MapRoute(
                    name: "signals",
                    template: tool.Url,
                    defaults: null,
                    constraints: null,
                    dataTokens: new { key = "signals" }
                );
                app.UseRouter(routesBuilderRecorder.Build());
            }
            tool = GetTool(kraftGlobalConfigurationSettings, "errors");
            if (tool != null && tool.Enabled)//Errors enabled from configuration
            {
            }
            tool = GetTool(kraftGlobalConfigurationSettings, "profiler");
            if (tool != null && tool.Enabled)//Profiler enabled from configuration
            {
            }

            //if (true)//Errors enabled from configuration
            //if (true)//Profiler enabled from configuration

        }

        public static ToolSettings GetTool(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, string kind)
        {
            foreach (ToolSettings tool in kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.Tools)
            {
                if (tool.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase))
                {
                    return tool;
                }
            }
            return null;
        }

    }
}
