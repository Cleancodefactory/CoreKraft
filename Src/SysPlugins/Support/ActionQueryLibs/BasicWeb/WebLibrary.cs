﻿using Ccf.Ck.Models.DirectCall;
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
using Ccf.Ck.Libs.Logging;
using System.IO;

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.BasicWeb
{
    [Library("basicweb",LibraryContextFlags.MainNode)]
    public class WebLibrary<HostInterface> : IActionQueryLibrary<HostInterface> where HostInterface : class
    {
        private object _LockObject = new Object();
        private HttpClient _Http = null;
        private HttpClientHandler _Handler = null;

        public WebLibrary() {
            var handler = new HttpClientHandler();
            _Handler = handler;
            handler.UseProxy = false; 
            _Http = new HttpClient(handler, true);
            _disposables.Add(_Http);
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
                case nameof(WGetFile):
                    return WGetFile;
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
        [Function(nameof(BuildQueryString), "Converts key/value pair of parameters to query string")]
        [ParameterPattern(1, "dict", "Contains the query parameters as key/value pairs", TypeFlags.String, TypeFlags.Object)]
        [Result("Returns the url encoded string", TypeFlags.String| TypeFlags.Bool)]
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

        [Function(nameof(UnbuildQueryString), "Converts a query string to dictionary")]
        [Parameter(1, "querystring", "Contains the query parameters as query string", TypeFlags.String)]
        [Result("Returns a dictionary populated from the query string", TypeFlags.Dict)]
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

        /// <summary>
        /// WGetJson(url, Dict of queryparams): dict
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(WGetFile), "Makes get request and returns the result as json ")]
        [Parameter(0, "url", "Fully qualified url", TypeFlags.String)]
        [Parameter(1, "name", "Contains an optional name (and filename) for the PostedFile object", TypeFlags.String| TypeFlags.Null)]
        [Parameter(2, "dict", "Contains the optional query parameters as Dict", TypeFlags.Dict | TypeFlags.Null)]
        [Parameter(3, "acceptedtypes", "Contains the optional list of accepted content types as List of string values", TypeFlags.List | TypeFlags.Null)]
        [Result("Returns the fetched content as PostedFile or Error", TypeFlags.PostedFile | TypeFlags.Error)]
        public ParameterResolverValue WGetFile(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length < 1) throw new ArgumentException("Not enough arguments to construct request");
            var url = Convert.ToString(args[0].Value);
            UriBuilder uri = new UriBuilder(url);
            string name = null;
            if (args.Length > 1)
            {
                name = Convert.ToString(args[1].Value);
            }
            if (args.Length > 2)
            {
                if (args[2].Value is Dictionary<string, ParameterResolverValue> pdict)
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);
                    foreach (var kv in pdict)
                    {
                        query[kv.Key] = Convert.ToString(kv.Value.Value);
                    }
                    uri.Query = query.ToString();
                }
            }
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
            string str;
            if (args.Length > 3)
            {
                if (args[3].Value is List<ParameterResolverValue> acceptTypes)
                {
                    for (int i = 0; i < acceptTypes.Count;i++)
                    {
                        str = Convert.ToString(acceptTypes[i].Value);
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(str));
                        }
                    }
                    
                }
            }

            //var accpets = new MediaTypeWithQualityHeaderValue("application/json");
            //msg.Headers.Accept.Add(accpets);

            using var respose = _Http.SendAsync(msg).Result;
            if (respose.StatusCode == HttpStatusCode.OK)
            {
                string mt = respose.Content.Headers.ContentType?.MediaType;
                string filename = respose.Content.Headers.ContentDisposition?.FileName;
                if (string.IsNullOrWhiteSpace(filename)) filename = null;
                var strm = respose.Content.ReadAsStreamAsync().Result;
                PostedFile pf = new PostedFile(mt != null ? mt : "application/octets-stream", strm.Length, name, filename, s => s as Stream, strm);
                return new ParameterResolverValue(pf);
            }
            else
            {
                return Error.Create($"HTTP error: {Convert.ToInt32(respose.StatusCode)}");
            }
        }

        private Regex reJSONMedia = new Regex("^.+/json.*$");
        /// <summary>
        /// WGetJson(url, Dict of queryparams): dict
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(WGetJson), "Makes get request and returns the result as json ")]
        [Parameter(1, "url", "Fully qualified url", TypeFlags.String)]
        [Parameter(2, "dict", "Contains the query parameters as Dict", TypeFlags.Dict | TypeFlags.Null)]
        [Result("Returns parsed json as dictionary from the http call", TypeFlags.Dict)]
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
            using var respose = _Http.SendAsync(msg).Result;
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

        [Function(nameof(WGetString), "Makes get request and returns the result as string ")]
        [Parameter(1, "url", "Fully qualified url", TypeFlags.String)]
        [Parameter(2, "dict", "Contains the query parameters as a Dict", TypeFlags.Dict | TypeFlags.Null)]
        [Result("Returns the string result from the http call", TypeFlags.String)]
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
            using var respose = _Http.SendAsync(msg).Result;
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

        [Function(nameof(WPostJson), "Makes post json request and returns the result as dictionary")]
        [Parameter(1, "url", "Fully qualified url", TypeFlags.String)]
        [Parameter(2, "data", "Contains the post parameters as a Dict", TypeFlags.Dict | TypeFlags.Null)]
        [Parameter(3, "qry", "Contains optional query string parameters as a Dict", TypeFlags.Dict | TypeFlags.Null)]
        [Result("Returns the result as dictionary from the http call", TypeFlags.Dict)]
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
            using var respose = _Http.SendAsync(msg).Result;
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
