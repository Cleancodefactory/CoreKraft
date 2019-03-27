using Microsoft.AspNetCore.Http;
using Ccf.Ck.SysPlugins.Interfaces.Packet;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public abstract class HttpResponseBuilder : IHttpResponseBuilder
    {
        protected abstract void WriteToResponseHeaders(HttpContext context);

        protected abstract void WriteToResponseBody(HttpContext context);

        public void GenerateResponse(HttpContext context)
        {
            WriteToResponseHeaders(context);
            WriteToResponseBody(context);
        }
    }
}
