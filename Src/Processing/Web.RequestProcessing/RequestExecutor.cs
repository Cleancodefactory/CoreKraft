using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Execution;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.Utilities.NodeSetService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ccf.Ck.Processing.Web.Request
{
    public class RequestExecutor
    {
        readonly IServiceProvider _ServiceProvider;
        readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        readonly TransactionScopeContext _TransactionScope;
        readonly HttpContext _HttpContext;
        readonly INodeSetService _NodesSetService;
        readonly KraftModuleCollection _KraftModuleCollection;
        static bool _IsSystemInMaintenanceMode;

        public RequestExecutor(IServiceProvider serviceProvider, HttpContext httpContext, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _ServiceProvider = serviceProvider;
            _HttpContext = httpContext;
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            _TransactionScope = new TransactionScopeContext(_ServiceProvider.GetService<IServiceCollection>());
            _NodesSetService = _ServiceProvider.GetService<INodeSetService>();
            _KraftModuleCollection = _ServiceProvider.GetService<KraftModuleCollection>();
        }

        public void ExecuteReEntrance(IProcessingContext processingContext, bool callInMaintenance = false)
        {
            if (_IsSystemInMaintenanceMode && !callInMaintenance)
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.ServiceUnavailable, $"Currently the system is maintained. Please retry in few minutes.");
                return;
            }
            processingContext.InputModel.ProcessingContextRef = this;
            processingContext.Execute(_TransactionScope);
        }

        public async Task ExecuteAsync()
        {
            IRequestRecorder requestRecorder = _ServiceProvider.GetService<IRequestRecorder>();
            if (requestRecorder != null)
            {
                if (_HttpContext.Session.IsAvailable && _HttpContext.Session.GetInt32("recorder") == 1)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    await requestRecorder.HandleRequest(_HttpContext.Request);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            AbstractProcessorFactory processorFactory = new KraftProcessorFactory();
            IProcessorHandler processor = processorFactory.CreateProcessor(_HttpContext, _KraftModuleCollection, _NodesSetService);
            IProcessingContextCollection processingContexts = processor.GenerateProcessingContexts(_ServiceProvider.GetService<KraftGlobalConfigurationSettings>(), _KraftGlobalConfigurationSettings.GeneralSettings.KraftRequestFlagsKey);
            if (processingContexts == null)
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.InternalServerError, $"ExecuteAsync.CreateProcessingContexts returned null.");
                return;
            }


            Task[] tasks = new Task[processingContexts.Length];
            int i = 0;
            try
            {
                _IsSystemInMaintenanceMode = processingContexts.IsMaintenance;

                foreach (IProcessingContext processingContext in processingContexts.ProcessingContexts)
                {
                    tasks[i++] = Task.Run(() =>
                    {
                        ExecuteReEntrance(processingContext, processingContexts.IsMaintenance);
                    });
                }
                await Task.WhenAll(tasks);
            }
            finally
            {
                _IsSystemInMaintenanceMode = false;
                processor.GenerateResponse();
            }
        }
    }
}
