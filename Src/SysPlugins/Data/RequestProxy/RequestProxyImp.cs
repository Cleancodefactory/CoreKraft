using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Data.RequestProxy
{
    public class RequestProxyImp : DataLoaderClassicBase<RequestProxySynchronizeContextScopedImp>
    {
        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            //string baseUrl = execContext.DataLoaderContextScoped.CustomSettings["BaseUrl"];
            //ParameterResolverValue endpoint = execContext.Evaluate("endpoint");
            //ParameterResolverValue method = execContext.Evaluate("method");
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            //if (!(endpoint.Value is string))
            //{
            //    KraftLogger.LogError("HttpServiceImp endpoint parameter value must be string");
            //    throw new Exception("endpoint value must be string");
            //}
            //string url = baseUrl + endpoint.Value;
            string test = "delete later";
            result.Add(new Dictionary<string, object>()
            {
                { "key", test }
            });
            return result;
        }

        protected override object Write(IDataLoaderWriteContext execContext)
        {
            throw new NotImplementedException();
        }
    }
}
