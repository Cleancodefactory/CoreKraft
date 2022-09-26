using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
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

        public ProcessorBase(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _KraftModuleCollection = kraftModuleCollection;
            _HttpContext = httpContext;
            _RequestMethod = (ERequestMethod)Enum.Parse(typeof(ERequestMethod), httpContext.Request.Method);
            _RequestContentType = requestContentType;
            _ProcessingContextCollection = new ProcessingContextCollection(new List<IProcessingContext>());
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            //AntiforgeryService
            //KeyValuePair<string, string> cookie = httpContext.Request.Cookies.FirstOrDefault(c => c.Key.Contains("XSRF-TOKEN"));
            //if (cookie.Value != null)
            //{
            //    httpContext.Request.Headers.Add("RequestVerificationToken", new StringValues(cookie.Value));
            //}
        }

        public abstract void GenerateResponse();

        public abstract void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext);

        public abstract IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null);

        protected T GetBodyJson<T>(HttpRequest httpRequest) where T : new()
        {
            T result = default(T);
            using (TextReader reader = new StreamReader(httpRequest.Body, Encoding.UTF8))
            {
                if (typeof(T) == typeof(Dictionary<string, object>))
                {
                    if (httpRequest.ContentLength.HasValue && httpRequest.ContentLength.Value > 0)
                    {
                        //options.Converters.Add(new DictionaryStringObjectJsonConverter());
                        //result = JsonSerializer.Deserialize<T>(reader.ReadToEnd(), options);
                        var options = new JsonReaderOptions
                        {
                            AllowTrailingCommas = true,
                            CommentHandling = JsonCommentHandling.Skip
                        };
                        result = (T)DictionaryStringObjectJson.Deserialize(reader.ReadToEnd(), options);

                    }
                }
                else
                {
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip
                    };
                    result = JsonSerializer.Deserialize<T>(reader.ReadToEnd(), options);
                }

                if (result == null)
                {
                    result = new T();
                }
            }
            return result;
        }
    }
}