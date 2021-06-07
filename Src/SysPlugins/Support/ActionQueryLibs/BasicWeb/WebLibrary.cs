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
using Newtonsoft.Json;
using Ccf.Ck.Utilities.Json;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.BasicWeb
{
    public class WebLibrary<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        private object _LockObject = new Object();
        private HttpClient http = null;
        public WebLibrary() {
            http = new HttpClient();
            _disposables.Add(http);
        }
        #region IActionQueryLibrary
        public HostedProc<HostInterface> GetProc(string name)
        {
            switch (name)
            {
                case nameof(WGetJson):
                    return WGetJson;
                case nameof(WPostJson):
                    return WPostJson;
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
        /// <summary>
        /// WGetJson(url, Dict of queryparams): dict
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ParameterResolverValue WGetJson(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("Not enough arguments to construct request");
            var url = Convert.ToString(args[0].Value);
            UriBuilder uri = new UriBuilder(url);
            if (args.Length > 1) {
                if (args[1].Value is Dictionary<string, ParameterResolverValue> pdict) {
                    var query = HttpUtility.ParseQueryString(string.Empty);
                    foreach (var kv in pdict) {
                        query[kv.Key] = Convert.ToString(kv.Value.Value);
                    }
                    uri.Query = query.ToString();
                }
            }
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
            var accpets = new MediaTypeWithQualityHeaderValue("application/json");
            msg.Headers.Accept.Add(accpets);
            using var respose = http.SendAsync(msg).Result;
            if (respose.StatusCode == HttpStatusCode.OK) {
                var mt = respose.Content.Headers.ContentType?.MediaType;
                if (mt != null && reJSONMedia.IsMatch(mt)) {
                    var jsonstring = respose.Content.ReadAsStringAsync().Result;
                    // Utf8JsonReader x = new Utf8JsonReader()
                    try {
                        //var kirech = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonstring);
                        var kirech = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonstring, new DictionaryConverter());
                        return DefaultLibraryBase<HostInterface>.ConvertFromGenericData(kirech);
                    } catch (Exception) {
                        return Error.Create("Cannot parse the returned content.");
                    }
                } else {
                    return Error.Create($"Unsupported result media: {mt}");
                }
            } else {
                return Error.Create($"HTTP error: {Convert.ToInt32(respose.StatusCode)}");
            }
        }

        public ParameterResolverValue WPostJson(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length < 1) throw new ArgumentException("Not enough arguments to construct request");
            var url = Convert.ToString(args[0].Value);
            UriBuilder uri = new UriBuilder(url);
            StringContent sc = null;
            if (args.Length > 1) {
                if (args[1].Value is Dictionary<string, ParameterResolverValue> pdict) {
                    var gendata = DefaultLibraryBase<HostInterface>.ConvertToGenericData(pdict);
                    var postdata = JsonConvert.SerializeObject(gendata);
                    sc = new StringContent(postdata, Encoding.UTF8, "application/json");
                }
            }
            if (args.Length > 2) {
                if (args[2].Value is Dictionary<string, ParameterResolverValue> pdict) {
                    var query = HttpUtility.ParseQueryString(string.Empty);
                    foreach (var kv in pdict) {
                        query[kv.Key] = Convert.ToString(kv.Value.Value);
                    }
                    uri.Query = query.ToString();
                }
            }
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri.Uri);
            if (sc != null) msg.Content = sc;
            var accpets = new MediaTypeWithQualityHeaderValue("application/json");
            msg.Headers.Accept.Add(accpets);
            using var respose = http.SendAsync(msg).Result;
            if (respose.StatusCode == HttpStatusCode.OK) {
                var mt = respose.Content.Headers.ContentType?.MediaType;
                if (mt != null && reJSONMedia.IsMatch(mt)) {
                    var jsonstring = respose.Content.ReadAsStringAsync().Result;
                    // Utf8JsonReader x = new Utf8JsonReader()
                    try {
                        //var kirech = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonstring);
                        var kirech = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonstring, new DictionaryConverter());
                        return DefaultLibraryBase<HostInterface>.ConvertFromGenericData(kirech);
                    } catch (Exception) {
                        return Error.Create("Cannot parse the returned content.");
                    }
                } else {
                    return Error.Create($"Unsupported result media: {mt}");
                }
            } else {
                return Error.Create($"HTTP error: {Convert.ToInt32(respose.StatusCode)}");
            }
        }
        #endregion

    }
}
