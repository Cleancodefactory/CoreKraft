﻿using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Execution;
using Ccf.Ck.Processing.Web.Request;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using Ccf.Ck.Utilities.NodeSetService;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ccf.Ck.Web.Middleware
{
    internal class KraftMiddleware
    {
        internal static RequestDelegate ExecutionDelegate(IApplicationBuilder builder, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            IAntiforgery antiforgeryService = builder.ApplicationServices.GetService<IAntiforgery>();
            RequestDelegate requestDelegate = async httpContext =>
            {
                RequestExecutor requestExecutor = new RequestExecutor(builder.ApplicationServices, httpContext, kraftGlobalConfigurationSettings);
                await requestExecutor.ExecuteAsync();

                //AntiforgeryService
                //if (HttpMethods.IsPost(httpContext.Request.Method))
                //{
                //    await antiforgeryService.ValidateRequestAsync(httpContext);
                //}
            };
            return requestDelegate;
        }

        internal static Func<Ccf.Ck.Models.DirectCall.InputModel, Task<Ccf.Ck.Models.DirectCall.ReturnModel>> ExecutionDelegateDirect(IApplicationBuilder builder, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            Func<Ccf.Ck.Models.DirectCall.InputModel, Task<Ccf.Ck.Models.DirectCall.ReturnModel>> directDelegate = (inputModel) =>
            {
                var transactionScope = new TransactionScopeContext(builder.ApplicationServices.GetService<IServiceCollection>());
                var nodesSetService = builder.ApplicationServices.GetService<INodeSetService>();
                var kraftModuleCollection = builder.ApplicationServices.GetService<KraftModuleCollection>();
                Models.DirectCall.ReturnModel returnModel = null;
                DirectCallHandler dcHandler = new DirectCallHandler(inputModel, kraftModuleCollection, nodesSetService);
                IProcessingContextCollection processingContextCollection = dcHandler.GenerateProcessingContexts(kraftGlobalConfigurationSettings, null);
                foreach (IProcessingContext processingContext in processingContextCollection.ProcessingContexts)
                {
                    dcHandler.Execute(processingContext, transactionScope);
                    returnModel = new Models.DirectCall.ReturnModel
                    {
                        Data = processingContext.ReturnModel.Data,
                        BinaryData = processingContext.ReturnModel.BinaryData,
                        IsSuccessful = processingContext.ReturnModel.Status.IsSuccessful
                    };
                    return Task.FromResult(returnModel);
                }
                return Task.FromResult(returnModel);
            };
            return directDelegate;
        }
    }
}
