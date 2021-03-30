using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.ContextBasket;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text;

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
            //image / jpeg
            if (_ProcessingContextCollection.ProcessingContexts.First().ReturnModel.BinaryData is IPostedFile postedFile)
            {
                if (!string.IsNullOrWhiteSpace(postedFile.ContentType))
                {
                    //For known content types create the etag
                    string etag = Ccf.Ck.Utilities.Generic.Utilities.GenerateETag(Encoding.UTF8.GetBytes(postedFile.Length + postedFile.FileName));
                    CacheManagement.HandleEtag(context, etag);
                }
            }
        }

        //response.ContentType = "image"; Coming from SendFileAsync

        protected override void WriteToResponseBody(HttpContext context)
        {
            //Etag controls it and we don't need body
            if (context.Response.StatusCode == StatusCodes.Status304NotModified)
            {
                return;
            }
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
