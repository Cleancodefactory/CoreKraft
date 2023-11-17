using Microsoft.AspNetCore.Http;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using Ccf.Ck.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Ccf.Ck.SysPlugins.Interfaces;
using System.Linq;
using Ccf.Ck.Models.ContextBasket;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public abstract class HttpResponseBuilder : IHttpResponseBuilder
    {
        private KraftGlobalConfigurationSettings _KraftGlobalConfiguration;
        protected IProcessingContextCollection _ProcessingContextCollection;
        public HttpResponseBuilder(IProcessingContextCollection processingContextCollection)
        {
            _KraftGlobalConfiguration = processingContextCollection.ProcessingContexts?.FirstOrDefault()?.InputModel.KraftGlobalConfigurationSettings;
            _ProcessingContextCollection = processingContextCollection;
        }
        protected abstract void WriteToResponseHeaders(HttpContext context);

        protected abstract void WriteToResponseBody(HttpContext context);

        public void GenerateResponse(HttpContext context)
        {
            if (_KraftGlobalConfiguration != null && _KraftGlobalConfiguration.GeneralSettings.CorsAllowedOrigins)
            {
                if (!context.Response.HasStarted)
                {
                    HttpResponse response = context.Response;
                    //Cors
                    response.Headers["Access-Control-Allow-Origin"] = "*";
                }
            }
            WriteToResponseHeaders(context);
            WriteToResponseBody(context);
        }
    }
}
