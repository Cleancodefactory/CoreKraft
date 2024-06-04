using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.NodeRequest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public class BinaryResponseBuilder : HttpResponseBuilder
    {

        public BinaryResponseBuilder(ProcessingContextCollection processingContextCollection) : base(processingContextCollection)
        {
        }

        protected override void WriteToResponseHeaders(HttpContext context)
        {
            HttpResponse response = context.Response;
            //image / jpeg / video
            if (_ProcessingContextCollection.ProcessingContexts.First().ReturnModel.BinaryData is IPostedFile postedFile)
            {
                if (HttpMethods.IsHead(context.Request.Method))
                {
                    context.Response.ContentLength = postedFile.Length;
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(postedFile.ContentType))
                {
                    response.ContentType = postedFile.ContentType;
                    response.ContentLength = postedFile.Length;
                    response.Headers.AcceptRanges = "bytes";
                    response.Headers["Access-Control-Allow-Origin"] = "*";
                    response.Headers["Access-Control-Allow-Headers"] = "Range";
                    response.Headers["Access-Control-Expose-Headers"] = "Content-Range, Accept-Ranges";

                    string etag = Ccf.Ck.Utilities.Generic.Utilities.GenerateETag(Encoding.UTF8.GetBytes(postedFile.Length + postedFile.FileName));
                    if (etag != null)
                    {
                        CacheManagement.HandleEtag(context, etag);
                        RequestHeaders requestHeaders = context.Request.GetTypedHeaders();
                        var ifNoneMatch = requestHeaders.IfNoneMatch;

                        if (ifNoneMatch?.Any() == true && ifNoneMatch.Contains(new EntityTagHeaderValue(Ccf.Ck.Utilities.Generic.Utilities.WithQuotes(etag))))
                        {
                            context.Response.StatusCode = StatusCodes.Status304NotModified;
                        }
                    }
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }

        protected override void WriteToResponseBody(HttpContext context)
        {
            HttpResponse response = context.Response;
            HttpRequest request = context.Request;

            //Etag controls it and we don't need body
            if (response.StatusCode == StatusCodes.Status304NotModified || response.StatusCode == StatusCodes.Status404NotFound)
            {
                return;
            }
            if (_ProcessingContextCollection.ProcessingContexts.First().ReturnModel.BinaryData is IPostedFile postedFile)
            {
                ICollection<RangeItemHeaderValue> ranges = request.GetTypedHeaders().Range?.Ranges;

                if (ranges != null && ranges.Count == 1 && ranges.First().From.HasValue && ranges.First().To.HasValue)
                {
                    response.StatusCode = StatusCodes.Status206PartialContent;
                    RangeItemHeaderValue range = ranges.First();

                    // Set Content-Range header
                    if (response.Headers.ContainsKey("Content-Range"))
                    {
                        response.Headers["Content-Range"] = $"bytes {range.From}-{range.To}/{postedFile.Length}";
                    }
                    else
                    {
                        response.Headers.Append("Content-Range", $"bytes {range.From}-{range.To}/{postedFile.Length}");
                    }

                    // Set content length for the range
                    response.ContentLength = range.To - range.From + 1;

                    byte[] data = ReadStreamIntoByteArray(postedFile.OpenReadStream(), range.From.Value, range.To.Value);
                    // Write the specified range of binary data to the response
                    response.Body.Write(data);
                }
                else
                {
                    using (System.IO.Stream pfs = postedFile.OpenReadStream())
                    {
                        pfs.CopyTo(response.Body);
                    }
                }
            }
        }

        private static byte[] ReadStreamIntoByteArray(Stream stream, long from, long to)
        {
            if (from < 0 || to < from || stream == null || !stream.CanSeek || !stream.CanRead)
            {
                throw new ArgumentException("Invalid arguments");
            }

            byte[] buffer = new byte[to - from + 1];

            stream.Seek(from, SeekOrigin.Begin);

            int bytesRead = 0;
            int totalBytesRead = 0;

            while (totalBytesRead < buffer.Length &&
                   (bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
            }

            return buffer;
        }
    }
}
