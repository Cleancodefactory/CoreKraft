using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

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

        protected abstract Task WriteToResponseBodyAsync(HttpContext context);

        public void GenerateResponse(HttpContext context)
        {
            if (_KraftGlobalConfiguration != null && _KraftGlobalConfiguration.GeneralSettings.CorsAllowedOrigins.Enabled)
            {
                if (!context.Response.HasStarted)
                {
                    HttpResponse response = context.Response;
                    //Cors
                    response.Headers["Access-Control-Allow-Origin"] = new[] { _KraftGlobalConfiguration.GeneralSettings.CorsAllowedOrigins.GetAllowedOrigins(context.Request) };
                    response.Headers["Access-Control-Allow-Methods"] = new[] { _KraftGlobalConfiguration.GeneralSettings.CorsAllowedOrigins.GetAllowMethods() };
                }
            }
            WriteToResponseHeaders(context);
            WriteToResponseBodyAsync(context).Wait();
        }
    }
}
