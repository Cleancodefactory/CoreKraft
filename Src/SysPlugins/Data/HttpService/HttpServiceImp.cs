using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Data.HttpService
{
    public class HttpServiceImp : DataLoaderClassicBase<HttpServiceSynchronizeContextScopedImp>
    {
        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            string baseUrl = execContext.DataLoaderContextScoped.CustomSettings["BaseUrl"];
            ParameterResolverValue endpoint = execContext.Evaluate("endpoint");
            ParameterResolverValue method = execContext.Evaluate("method");
            var result = new List<Dictionary<string, object>>();
            if (!(endpoint.Value is string))
            {
                KraftLogger.LogError("HttpServiceImp endpoint parameter value must be string");
                throw new Exception("endpoint value must be string");
            }
            string url = baseUrl + endpoint.Value;
            var obj = this.GetHttpContent(url);
            result.Add(new Dictionary<string, object>()
            {
                { "key", obj }
            });
            return result;
        }

        protected override object Write(IDataLoaderWriteContext execContext)
        {
            return null;
        }

        private async Task<string> GetHttpContent(string url)
        {
            KraftLogger.LogTrace(url);
            try
            {
                using (var http = new HttpClient())
                {
                    var httpResponse = await http.GetAsync(url);
                    var httpContent = await httpResponse.Content.ReadAsStringAsync();
                    if (httpResponse.StatusCode.ToString().StartsWith("5") || httpResponse.StatusCode.ToString().StartsWith("4"))
                    {
                        KraftLogger.LogWarning("Recieved status code:" + httpResponse.StatusCode, httpResponse);
                    }
                    return httpContent;
                }
            }
            catch (Exception ex)
            {
                KraftLogger.LogError(ex, "Method: GetHttpContent");
                throw;
            }
        }
    }
}
