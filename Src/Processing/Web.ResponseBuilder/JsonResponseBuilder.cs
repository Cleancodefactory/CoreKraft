﻿using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ccf.Ck.Processing.Web.ResponseBuilder
{
    public class JsonResponseBuilder : HttpResponseBuilder
    {
        public JsonResponseBuilder(IProcessingContextCollection processingContextCollection) : base(processingContextCollection)
        {
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
                response.ContentType = "application/json";
            }
        }

        protected override async Task WriteToResponseBodyAsync(HttpContext context)
        {
            string result = string.Empty;
            foreach (IProcessingContext processingContext in _ProcessingContextCollection.ProcessingContexts)
            {
                if (processingContext.ReturnModel.Data != null)
                {
                    result = JsonSerializer.Serialize(processingContext.ReturnModel.Data);
                    break; //we are handling only one context (no packaging possible)
                }
            }
            await context.Response.WriteAsync(result);
        }
    }
}
