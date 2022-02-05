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
using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using static Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes.BaseAttribute;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.BasicWeb
{
    public class WebLibrary<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        private object _LockObject = new Object();
        private HttpClient http = null;
        private const string LIBRARYNAME = "WebLibrary";

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
                case nameof(WGetString):
                    return WGetString;
                case nameof(WPostJson):
                    return WPostJson;
                case nameof(BuildQueryString):
                    return BuildQueryString;
                case nameof(UnbuildQueryString):
                    return UnbuildQueryString;
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
        [Function(nameof(BuildQueryString), "Converts key/value pair of parameters to query string", LIBRARYNAME)]
        [ParameterPattern(1, "dict", "Contains the query parameters as key/value pairs", TypeEnum.String, TypeEnum.Object)]
        [Result("Returns the url encoded string", TypeEnum.String| TypeEnum.Bool)]
        public ParameterResolverValue BuildQueryString(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Wrong number of arguments for BuildQueyString");
            
            if (args[0].Value is Dictionary<string, ParameterResolverValue> pdict) {
                var query = HttpUtility.ParseQueryString(string.Empty);
                foreach (var kv in pdict) {
                    query[kv.Key] = Convert.ToString(kv.Value.Value);
                }
                return new ParameterResolverValue(query.ToString());
            } else {
                throw new ArgumentException("The argument for BuildQueyString must be dictionary");
            }
        }

        [Function(nameof(UnbuildQueryString), "Converts a query string to dictionary", LIBRARYNAME)]
        [Parameter(1, "querystring", "Contains the query parameters as query string", TypeEnum.String)]
        [Result("Returns a dictionary populated from the query string", TypeEnum.Dict)]
        public ParameterResolverValue UnbuildQueryString(HostInterface ctx, ParameterResolverValue[] args) {
            if (args.Length != 1) throw new ArgumentException("UnbuildQueryString requires exactly one argument.");
            string s;
            if (args[0].Value != null) {
                s = args[0].Value.ToString();
            } else {
                return new ParameterResolverValue(null);
            }
            var coll = HttpUtility.ParseQueryString(s);
            var dic = new Dictionary<string, ParameterResolverValue>();
            if (coll.HasKeys()) {
                foreach (var k in coll.AllKeys) {
                    var vals = coll.GetValues(k);
                    if (vals != null) {
                        if (vals.Length > 1) {
                            dic.Add(k, new ParameterResolverValue(new ValueList<string>(vals)));
                        } else {
                            dic.Add(k, new ParameterResolverValue(vals[0]));
                        }
                    }
                }
            }
            return new ParameterResolverValue(dic);
        }

        private Regex reJSONMedia = new Regex("^.+/json.*$");
        /// <summary>
        /// WGetJson(url, Dict of queryparams): dict
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(WGetJson), "Makes get request and returns the result as json ", LIBRARYNAME)]
        [Parameter(1, "url", "Fully qualified url", TypeEnum.String)]
        [ParameterPattern(2, "dict", "Contains the query parameters as key/value pairs", TypeEnum.String, TypeEnum.Object )]
        [Result("Returns parsed json as dictionary from the http call", TypeEnum.Dict)]
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

        [Function(nameof(WGetString), "Makes get request and returns the result as string ", LIBRARYNAME)]
        [Parameter(1, "url", "Fully qualified url", TypeEnum.String)]
        [ParameterPattern(2, "dict", "Contains the query parameters as key/value pairs", TypeEnum.String, TypeEnum.Object)]
        [Result("Returns the string result from the http call", TypeEnum.String)]
        public ParameterResolverValue WGetString(HostInterface ctx, ParameterResolverValue[] args) {
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
            var accpets = new MediaTypeWithQualityHeaderValue("application/octet-stream");
            msg.Headers.Accept.Add(accpets);
            using var respose = http.SendAsync(msg).Result;
            if (respose.StatusCode == HttpStatusCode.OK) {
                try {
                    var rstring = respose.Content.ReadAsStringAsync().Result;
                    return new ParameterResolverValue(rstring);
                } catch (Exception) {
                    return Error.Create("Cannot parse the returned content.");
                }
            } else {
                return Error.Create($"HTTP error: {Convert.ToInt32(respose.StatusCode)}");
            }
        }

        [Function(nameof(WPostJson), "Makes post json request and returns the result as dictionary", LIBRARYNAME)]
        [Parameter(1, "url", "Fully qualified url", TypeEnum.String)]
        [ParameterPattern(2, "dict", "Contains the post parameters as key/value pairs", TypeEnum.String, TypeEnum.Object)]
        [Result("Returns the result as dictionary from the http call", TypeEnum.Dict)]
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
