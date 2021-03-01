using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Web.Request;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.Utilities.NodeSetService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            ProcessorSignal processorSignal = new ProcessorSignal(httpContext, kraftModuleCollection, ESupportedContentTypes.JSON, nodeSetService, _KraftGlobalConfigurationSettings);
            IProcessingContextCollection processingContextCollection = processorSignal.GenerateProcessingContexts(string.Empty, new SecurityModelMock(_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection));
            RequestExecutor requestExecutor = new RequestExecutor(_ServiceProvider, httpContext, _KraftGlobalConfigurationSettings);
            foreach (IProcessingContext processingContext in processingContextCollection.ProcessingContexts)
            {
                requestExecutor.ExecuteReEntrance(processingContext);
            }
        }
    }
}
