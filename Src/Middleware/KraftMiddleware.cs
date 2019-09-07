using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Web.Request;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
