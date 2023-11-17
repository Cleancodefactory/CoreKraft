using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public class TextResponseBuilder : HttpResponseBuilder
    {
        private string _ContentType = "text/plain";

        public TextResponseBuilder(IProcessingContextCollection processingContextCollection, string contentType = null) : base(processingContextCollection)
        {
            if (contentType != null) _ContentType = contentType;
        }

        protected override void WriteToResponseHeaders(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                HttpResponse response = context.Response;

                // Disable caching for all Kraft responses
                response.Headers["Cache-Control"] = "no-cache, no-store";
                response.Headers["Pragma"] = "no-cache";
                response.Headers["Expires"] = "-1";

                //set json content type        
                response.ContentType = _ContentType;
            }
        }

        protected override void WriteToResponseBody(HttpContext context)
        {
            string result = string.Empty;
            foreach (IProcessingContext processingContext in _ProcessingContextCollection.ProcessingContexts)
            {
                if (processingContext.ReturnModel.Data != null)
                {
                    result = processingContext.ReturnModel.Data.ToString();
                    break; //we are handling only one context (no packaging possible)
                }
            }
            context.Response.WriteAsync(result).Wait();
        }
    }
}
