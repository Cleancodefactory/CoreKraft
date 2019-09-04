using Ccf.Ck.Processing.Execution;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Utilities.NodeSetService;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Net;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System;

namespace Ccf.Ck.Processing.Web.Request
{
    public class RequestExecutor
    {
        IServiceProvider _ServiceProvider;
        KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        TransactionScopeContext _TransactionScope;
        HttpContext _HttpContext;
        INodeSetService _NodesSetService;
        KraftModuleCollection _KraftModuleCollection;
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
                    tasks[i++] = Task.Run(() => {
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
