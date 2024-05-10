﻿using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.NodeRequest;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
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

        protected override void WriteToResponseBody(HttpContext context)
        {
            HttpResponse response = context.Response;
            HttpRequest request = context.Request;

            //Etag controls it and we don't need body
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

                //We can decide to split only big videos into ranges
                //if (postedFile.ContentType.Equals("video/mp4", comparisonType: StringComparison.OrdinalIgnoreCase))
                //{

                //}               
                response.Headers.AcceptRanges = "bytes";
                response.Headers.ContentRange = $"bytes */{postedFile.Length}";

                var ranges = request.GetTypedHeaders().Range?.Ranges;

                if (ranges != null && ranges.Count == 1 && ranges.First().From.HasValue && ranges.First().To.HasValue)
                {
                    RangeItemHeaderValue range = ranges.First();

                    // Set Content-Range header
                    response.Headers.Add("Content-Range", $"bytes {range.From}-{range.To}/{postedFile.ContentType.Length}");
                    // Set status code to 206 Partial Content
                    response.StatusCode = StatusCodes.Status206PartialContent;
                    // Set content length for the range
                    response.ContentLength = range.To - range.From + 1;

                    byte[] data = ReadStreamIntoByteArray(postedFile.OpenReadStream(), range.From.Value, range.To.Value);
                    // Write the specified range of binary data to the response
                    response.Body.Write(data, (int)range.From, (int)range.To.Value);
                }
                else
                {
                    response.StatusCode = StatusCodes.Status200OK;
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
