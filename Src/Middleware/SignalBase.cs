using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.Processing.Web.Request;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.Utilities.NodeSetService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static Ccf.Ck.Processing.Web.Request.BaseClasses.ProcessorBase;
namespace Ccf.Ck.Web.Middleware
{
    internal class SignalBase
    {
        protected IServiceProvider _ServiceProvider;
        protected KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;

        internal void ExecuteSignals(string module, string signal)
        {
            INodeSetService nodeSetService = _ServiceProvider.GetService<INodeSetService>();
            KraftModuleCollection kraftModuleCollection = _ServiceProvider.GetService<KraftModuleCollection>();

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "GET";
            RouteData routeData = new RouteData();
            routeData.Values.Add(Constants.RouteSegmentConstants.RouteModule, module);
            routeData.Values.Add(Constants.RouteSegmentConstants.RouteModuleSignalParameter, signal);
            routeData.DataTokens.Add("key", Constants.RouteSegmentConstants.RouteDataTokenSignalRead);

            httpContext.Features[typeof(IRoutingFeature)] = new RoutingFeature()
            {
                RouteData = routeData,
            };
            // {Security}
            // TODO - use builtin user defaults instead of the mock user when we are ready to fix the affected projects.
            ISecurityModel securityModel = new SecurityModelMock(_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection);
            KraftModule moduleConf = kraftModuleCollection.GetModule(module);
            IEnumerable<KraftModuleSignal> signals = moduleConf?.KraftModuleRootConf?.Signals;
            if (signals != null) {
                KraftModuleSignal ksignal =  signals.FirstOrDefault(confsig => string.Compare(confsig.Key, signal,true) == 0);
                if (ksignal != null && !string.IsNullOrEmpty(ksignal.RunAs)) {
                    securityModel = SecurityModelBuiltIn.Create(ksignal.RunAs, _KraftGlobalConfigurationSettings);
                }
            }
            ProcessorSignal processorSignal = new ProcessorSignal(httpContext, kraftModuleCollection, ESupportedContentTypes.JSON, nodeSetService, _KraftGlobalConfigurationSettings);
            IProcessingContextCollection processingContextCollection = processorSignal.GenerateProcessingContexts(string.Empty, securityModel);
            RequestExecutor requestExecutor = new RequestExecutor(_ServiceProvider, httpContext, _KraftGlobalConfigurationSettings);
            foreach (IProcessingContext processingContext in processingContextCollection.ProcessingContexts)
            {
                
                requestExecutor.ExecuteReEntrance(processingContext);
            }
        }
    }
}
