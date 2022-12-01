using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Execution;
using Ccf.Ck.Processing.Web.Request;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using Ccf.Ck.Utilities.NodeSetService;
using dotless.Core.Parser.Infrastructure;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
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
                bool? containsHistory = httpContext.GetRouteData()?.DataTokens?.Values?.Contains("history");
                if (containsHistory.HasValue && containsHistory.Value)
                {
                    //TODO User agent for search engine


                }
                else
                {
                    RequestExecutor requestExecutor = new RequestExecutor(builder.ApplicationServices, httpContext, kraftGlobalConfigurationSettings);
                    await requestExecutor.ExecuteAsync();

                    //AntiforgeryService
                    //if (HttpMethods.IsPost(httpContext.Request.Method))
                    //{
                    //    await antiforgeryService.ValidateRequestAsync(httpContext);
                    //}
                }
            };
            return requestDelegate;
        }

        internal static Func<Ccf.Ck.Models.DirectCall.InputModel, Ccf.Ck.Models.DirectCall.ReturnModel> ExecutionDelegateDirect(IApplicationBuilder builder, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            Func<Ccf.Ck.Models.DirectCall.InputModel, Ccf.Ck.Models.DirectCall.ReturnModel> directDelegate = (inputModel) =>
            {
                return Task.Run(() =>
                {
                    TransactionScopeContext transactionScope = new TransactionScopeContext(builder.ApplicationServices.GetService<IServiceCollection>());
                    INodeSetService nodesSetService = builder.ApplicationServices.GetService<INodeSetService>();
                    KraftModuleCollection kraftModuleCollection = builder.ApplicationServices.GetService<KraftModuleCollection>();
                    ReturnModel returnModel = null;
                    DirectCallHandler dcHandler = new DirectCallHandler(inputModel, kraftModuleCollection, nodesSetService, kraftGlobalConfigurationSettings);
                    IProcessingContextCollection processingContextCollection = dcHandler.GenerateProcessingContexts(null);
                    Stopwatch stopWatch = null;
                    if (kraftGlobalConfigurationSettings.EnvironmentSettings.IsDevelopment())
                    {
                        stopWatch = new Stopwatch();
                        stopWatch.Start();
                    }
                    foreach (IProcessingContext processingContext in processingContextCollection.ProcessingContexts)
                    {
                        dcHandler.Execute(processingContext, transactionScope);
                        returnModel = new Models.DirectCall.ReturnModel
                        {
                            Data = processingContext.ReturnModel.Data,
                            BinaryData = processingContext.ReturnModel.BinaryData,
                            IsSuccessful = processingContext.ReturnModel.Status.IsSuccessful
                        };
                        if (stopWatch != null)
                        {
                            stopWatch.Stop();
                            Console.WriteLine($"Directcall {processingContext.InputModel.Module}:{processingContext.InputModel.NodeSet}:{processingContext.InputModel.Nodepath} executed in {stopWatch.ElapsedMilliseconds} milliseconds");
                        }
                    }
                    return returnModel;
                }).Result;
            };
            return directDelegate;
        }
    }
}
