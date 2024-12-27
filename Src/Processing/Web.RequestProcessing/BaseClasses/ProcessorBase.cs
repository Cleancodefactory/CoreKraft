using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.Utilities.Json;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ccf.Ck.Processing.Web.Request.BaseClasses
{
    /// <summary>
    /// Ultimate Base common for WEB and non-WEB requests
    /// </summary>
    public abstract class ProcessorBase : IProcessorHandler
    {
        public enum ERequestMethod
        {
            GET, HEAD, POST, PUT, PATCH, DELETE, OPTIONS, TRACE
        }

        public enum ESupportedContentTypes
        {
            JSON, FORM_MULTIPART, FORM_URLENCODED, UNKNOWN
        }

        protected ERequestMethod _RequestMethod { get; private set; }
        protected ESupportedContentTypes _RequestContentType { get; private set; }
        protected KraftModuleCollection _KraftModuleCollection;
        protected HttpContext _HttpContext;
        protected IProcessingContextCollection _ProcessingContextCollection;
        protected KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        private bool _PreserveBody;

        public ProcessorBase(HttpContext httpContext, 
            KraftModuleCollection kraftModuleCollection, 
            ESupportedContentTypes requestContentType, 
            KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, 
            bool preserveBody = false)
        {
            _KraftModuleCollection = kraftModuleCollection;
            _HttpContext = httpContext;
            _RequestMethod = (ERequestMethod)Enum.Parse(typeof(ERequestMethod), httpContext.Request.Method);
            _RequestContentType = requestContentType;
            _ProcessingContextCollection = new ProcessingContextCollection(new List<IProcessingContext>());
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            _PreserveBody = preserveBody;
            //AntiforgeryService
            //KeyValuePair<string, string> cookie = httpContext.Request.Cookies.FirstOrDefault(c => c.Key.Contains("XSRF-TOKEN"));
            //if (cookie.Value != null)
            //{
            //    httpContext.Request.Headers.Add("RequestVerificationToken", new StringValues(cookie.Value));
            //}
        }

        public abstract void GenerateResponse();

        protected string RequestBody { get; set; }

        public abstract void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext);

        public abstract IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null);

        protected async Task<T> GetBodyJsonAsync<T>(HttpRequest httpRequest) where T : new()
        {
            T result = default(T);

            // Ensure we can read the body
            if (httpRequest.Body.CanSeek)
            {
                httpRequest.Body.Seek(0, SeekOrigin.Begin);
            }

            using (StreamReader reader = new StreamReader(httpRequest.Body, Encoding.UTF8))
            {
                string requestBody = await reader.ReadToEndAsync();

                if (typeof(T) == typeof(Dictionary<string, object>))
                {
                    if (httpRequest.ContentLength.HasValue && httpRequest.ContentLength.Value > 0)
                    {
                        var options = new JsonReaderOptions
                        {
                            AllowTrailingCommas = true,
                            CommentHandling = JsonCommentHandling.Skip
                        };

                        result = (T)(object)DictionaryStringObjectJson.Deserialize(requestBody, options);
                    }
                }
                else
                {
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip
                    };
                    result = JsonSerializer.Deserialize<T>(requestBody, options);
                }

                if (result == null)
                {
                    result = new T();
                }
                if (_PreserveBody)
                {
                    RequestBody = requestBody;
                }
            }
            return result;
        }
    }
}