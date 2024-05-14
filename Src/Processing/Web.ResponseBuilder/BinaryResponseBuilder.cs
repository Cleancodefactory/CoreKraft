using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.NodeRequest;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (_ProcessingContextCollection.ProcessingContexts.First().ReturnModel.BinaryData is IPostedFile postedFile)
            {
                if (!string.IsNullOrWhiteSpace(postedFile.ContentType))
                {
                    string etag = Ccf.Ck.Utilities.Generic.Utilities.GenerateETag(Encoding.UTF8.GetBytes(postedFile.Length + postedFile.FileName));
                    CacheManagement.HandleEtag(context, etag);
                }
            }
        }

        protected override void WriteToResponseBody(HttpContext context)
        {
            HttpResponse response = context.Response;
            HttpRequest request = context.Request;

            if (response.StatusCode == StatusCodes.Status304NotModified)
            {
                return;
            }
            if (_ProcessingContextCollection.ProcessingContexts.First().ReturnModel.BinaryData is IPostedFile postedFile)
            {
                if (!string.IsNullOrWhiteSpace(postedFile.ContentType))
                {
                    response.ContentType = postedFile.ContentType;
                }

                response.ContentLength = postedFile.Length;
                response.Headers.AcceptRanges = "bytes";

                var ranges = request.GetTypedHeaders().Range?.Ranges;

                if (ranges != null && ranges.Count == 1 && ranges.First().From.HasValue && ranges.First().To.HasValue)
                {
                    WriteRangeResponseAsync(response, postedFile, ranges.First()).Wait();
                }
                else
                {
                    WriteFullResponseAsync(response, postedFile).Wait();
                }
            }
        }

        private async Task WriteRangeResponseAsync(HttpResponse response, IPostedFile postedFile, RangeItemHeaderValue range)
        {
            long from = range.From!.Value;
            long to = range.To!.Value;

            response.Headers.ContentRange = $"bytes {from}-{to}/{postedFile.Length}";
            response.StatusCode = StatusCodes.Status206PartialContent;
            response.ContentLength = to - from + 1;

            byte[] data = await ReadStreamIntoByteArrayAsync(postedFile.OpenReadStream(), from, to);
            await response.Body.WriteAsync(data, 0, data.Length);
        }

        private async Task WriteFullResponseAsync(HttpResponse response, IPostedFile postedFile)
        {
            response.StatusCode = StatusCodes.Status200OK;
            using (Stream stream = postedFile.OpenReadStream())
            {
                await stream.CopyToAsync(response.Body);
            }
        }

        private static async Task<byte[]> ReadStreamIntoByteArrayAsync(Stream stream, long from, long to)
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
                   (bytesRead = await stream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
            }

            return buffer;
        }
    }
}
