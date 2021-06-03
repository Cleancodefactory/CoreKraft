using Ccf.Ck.Models.DirectCall;
using dcall = Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using Ccf.Ck.Models.NodeRequest;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.BasicWeb
{
    public class WebLibrary<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        private object _LockObject = new Object();
        private HttpClient http = null;
        public WebLibrary() {
            http = new HttpClient();
        }
        #region IActionQueryLibrary
        public HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                
            }
            return null;
        }

        public SymbolSet GetSymbols()
        {
            return new SymbolSet("Basic Web requests library (no symbols)", null);
        }

        private List<object> _disposables = new List<object>();
        public void ClearDisposables()
        {
            lock (_LockObject)
            {
                for (int i = 0; i < _disposables.Count; i++)
                {
                    if (_disposables[i] is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                }
                _disposables.Clear();
            }
        }
        #endregion

        

        #region Functions
        private Regex reJSONMedia = new Regex("^.+/json.*$");
        public ParameterResolverValue WGetJson(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("Not enough arguments to construct request");
            var url = Convert.ToString(args[0].Value);
            UriBuilder uri = new UriBuilder(url);
            if (args.Length > 1) {
                if (args[1].Value is Dictionary<string, ParameterResolverValue> pdict) {
                    var query = HttpUtility.ParseQueryString(string.Empty);
                    foreach (var kv in pdict) {
                        query[kv.Key] = Convert.ToString(kv.Value);
                    }
                    uri.Query = query.ToString();
                }
            }
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
            var accpets = new MediaTypeWithQualityHeaderValue("application/json");
            msg.Headers.Accept.Add(accpets);
            var respose = http.SendAsync(msg).Result;
            if (respose.StatusCode == HttpStatusCode.OK) {
                var mt = respose.Content.Headers.ContentType?.MediaType;
                if (mt != null && reJSONMedia.IsMatch(mt)) {
                    var jsonstring = respose.Content.ReadAsStringAsync().Result;
                    Utf8JsonReader x = new Utf8JsonReader()
                    JsonSerializer.Deserialize()
                }
            }
        }
        
        #endregion

    }
}
