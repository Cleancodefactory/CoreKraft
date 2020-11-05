using Microsoft.AspNetCore.Http;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Ccf.Ck.Utilities.Json;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.NodeRequest;

namespace Ccf.Ck.Processing.Web.Request.BaseClasses
{
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

        public ProcessorBase(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType)
        {
            _KraftModuleCollection = kraftModuleCollection;
            _HttpContext = httpContext;
            _RequestMethod = (ERequestMethod)Enum.Parse(typeof(ERequestMethod), httpContext.Request.Method);
            _RequestContentType = requestContentType;
            _ProcessingContextCollection = new ProcessingContextCollection(new List<IProcessingContext>());
            //AntiforgeryService
            //KeyValuePair<string, string> cookie = httpContext.Request.Cookies.FirstOrDefault(c => c.Key.Contains("XSRF-TOKEN"));
            //if (cookie.Value != null)
            //{
            //    httpContext.Request.Headers.Add("RequestVerificationToken", new StringValues(cookie.Value));
            //}
        }

        public abstract void GenerateResponse();

        public abstract void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext);

        public abstract IProcessingContextCollection GenerateProcessingContexts(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, string kraftRequestFlagsKey, ISecurityModel securityModel = null);

        protected T GetBodyJson<T>(HttpRequest httpRequest) where T: new()
        {
            T result = default(T);
            using (TextReader reader = new StreamReader(httpRequest.Body, Encoding.UTF8))
            {
                if (typeof(T) == typeof(Dictionary<string, object>))
                {
                    result = JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), new DictionaryConverter());
                }
                else
                {
                    result = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
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