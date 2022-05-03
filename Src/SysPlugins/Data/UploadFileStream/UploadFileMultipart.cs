using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Ccf.Ck.SysPlugins.Data.UploadFileStream.BaseClasses;
using Ccf.Ck.SysPlugins.Data.UploadFileStream.Utilities;

namespace Ccf.Ck.SysPlugins.Data.UploadFileStream
{
    class UploadFileMultipart : UploadFileBase
    {
        private static readonly FormOptions _DefaultFormOptions = new FormOptions();

        internal override async Task<Interfaces.ContextualBasket.IProcessingContext> Execute(HttpRequest request, Interfaces.ContextualBasket.IProcessingContext processingContext)
        {
            // Used to accumulate all the form url encoded key value pairs in the request.
            KeyValueAccumulator formAccumulator = new KeyValueAccumulator();
            
            string boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), _DefaultFormOptions.MultipartBoundaryLengthLimit);
            MultipartReader reader = new MultipartReader(boundary, request.Body);

            MultipartSection section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        string targetFilePath = Path.GetRandomFileName();
                        using (var targetStream = System.IO.File.Create(targetFilePath))
                        {
                            await section.Body.CopyToAsync(targetStream);
                        }
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        // Content-Disposition: form-data; name="key" value

                        // Do not limit the key name length here because the 
                        // multipart headers length limit is already in effect.
                        StringSegment key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        Encoding encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                        {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            string value = await streamReader.ReadToEndAsync();
                            if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = String.Empty;
                            }
                            formAccumulator.Append(key.ToString(), value);

                            if (formAccumulator.ValueCount > _DefaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {_DefaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
            //else
            //{
            //    processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = "Current node is null or not OfType(View)" });
            //    throw new InvalidDataException("HtmlViewSynchronizeContextLocalImp.CurrentNode is null or not OfType(View)");
            //}
            return await Task.FromResult(processingContext);
            //// Bind form data to a model
            //var formValueProvider = new FormValueProvider(
            //    BindingSource.Form,
            //    new FormCollection(formAccumulator.GetResults()),
            //    CultureInfo.CurrentCulture);
        }
    }
}
