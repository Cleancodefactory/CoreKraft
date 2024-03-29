﻿using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Utilities.Profiling;
using Ccf.Ck.Web.Middleware.Tools;
using dotless.Core.Parser.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Web.Middleware
{
    public class KraftToolsRouteBuilder
    {
        internal static void MakeRouters(IApplicationBuilder app, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            RouteHandler routesHandler;
            ToolSettings tool = GetTool(kraftGlobalConfigurationSettings, "recorder");//////////////////////////////Recorder/////////////////////////////
            if (tool != null)//Recorder enabled from configuration
            {
                kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.IsEnabled = tool.Enabled;
                routesHandler = new RouteHandler(RecorderDelegate.ExecutionDelegate(app, kraftGlobalConfigurationSettings));

                RouteBuilder routesBuilder = new RouteBuilder(app, routesHandler);

                //we expect the routing to be like this:
                //domain.com/recorder/0|1|2?lang=de
                routesBuilder.MapRoute(
                    name: "recorder",
                    template: tool.Url,
                    defaults: null,
                    constraints: null,
                    dataTokens: new { key = "recorder" }
                );
                app.UseRouter(routesBuilder.Build());
            }
            tool = GetTool(kraftGlobalConfigurationSettings, "signals");//////////////////////////////Signals/////////////////////////////
            if (tool != null)//Signals enabled from configuration
            {
                routesHandler = new RouteHandler(SignalDelegate.ExecutionDelegate(app, kraftGlobalConfigurationSettings));

                RouteBuilder routesBuilder = new RouteBuilder(app, routesHandler);

                //we expect the routing to be like this:
                //domain.com/signals/0|1|2?lang=de
                routesBuilder.MapRoute(
                    name: "signals",
                    template: tool.Url,
                    defaults: null,
                    constraints: null,
                    dataTokens: new { key = "signals" }
                );
                app.UseRouter(routesBuilder.Build());
            }
            tool = GetTool(kraftGlobalConfigurationSettings, "errors");//////////////////////////////Errors/////////////////////////////
            if (tool != null && tool.Enabled == false)//Errors not enabled -> register route to respond if requested
            {
                routesHandler = new RouteHandler(ErrorsDelegate.ExecutionDelegate(app, kraftGlobalConfigurationSettings));

                RouteBuilder routesBuilder = new RouteBuilder(app, routesHandler);

                //we expect the routing to be like this:
                //domain.com/tools/errors
                routesBuilder.MapRoute(
                    name: "errors",
                    template: tool.Url,
                    defaults: null,
                    constraints: null,
                    dataTokens: new { key = "errors" }
                );
                app.UseRouter(routesBuilder.Build());
            }
            tool = GetTool(kraftGlobalConfigurationSettings, "profiler");//////////////////////////////Profiler/////////////////////////////
            if (tool != null && tool.Enabled)//Profiler enabled from configuration
            {
                app.UseBindKraftProfiler();                
            }
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
