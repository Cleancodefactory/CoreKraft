using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.ContextBasket;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public class BinaryResponseBuilder : HttpResponseBuilder
    {
        private ProcessingContextCollection _ProcessingContextCollection;

        public BinaryResponseBuilder(ProcessingContextCollection processingContextCollection)
        {
            _ProcessingContextCollection = processingContextCollection;
        }

        protected override void WriteToResponseHeaders(HttpContext context)
        {
            HttpResponse response = context.Response;

            // Disable caching for all Kraft responses
            response.Headers["Cache-Control"] = "no-cache, no-store";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "-1";
    
            //response.ContentType = "image"; Coming from SendFileAsync
        }

        protected override void WriteToResponseBody(HttpContext context)
        {
            if (_ProcessingContextCollection.ProcessingContexts.First().ReturnModel.BinaryData is IPostedFile postedFile)
            {
                if (!string.IsNullOrWhiteSpace(postedFile.ContentType))
                {
                    context.Response.ContentType = postedFile.ContentType;
                }

                context.Response.SendFileAsync(postedFile.FileName).Wait();
            }
        }
    }
}
