using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    internal class CacheManagement
    {
        internal static void HandleEtag (HttpContext httpContext, string calculatedEtag)
        {
            if (httpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var etag) && Ccf.Ck.Utilities.Generic.Utilities.WithQuotes(calculatedEtag) == etag)
            {
                //there is etag and it is equal
                httpContext.Response.StatusCode = StatusCodes.Status304NotModified;
                return;
            }
            else
            {
                //there is NO etag OR it is NOT equal
            }
            CacheControlHeaderValue cacheControlHeaderValue = new CacheControlHeaderValue();
            cacheControlHeaderValue.MaxAge = TimeSpan.FromDays(365);
            cacheControlHeaderValue.Public = true;

            httpContext.Response.GetTypedHeaders().CacheControl = cacheControlHeaderValue;
            httpContext.Response.Headers[HeaderNames.ETag] = new[] { Ccf.Ck.Utilities.Generic.Utilities.WithQuotes(calculatedEtag) };
        }
    }
}
