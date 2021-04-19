using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.Utilities.NodeSetService;
using Ccf.Ck.SysPlugins.Interfaces;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorMultipartEx : ProcessorNodeBase
    {
        public const int MAX_INDEX_LIMIT = 1000;
        public ProcessorMultipartEx(    HttpContext httpContext, 
                                        KraftModuleCollection kraftModuleCollection, 
                                        ESupportedContentTypes requestContentType, 
                                        INodeSetService nodeSetService, 
                                        KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) 
                                            : base(httpContext, kraftModuleCollection, requestContentType, nodeSetService, kraftGlobalConfigurationSettings)
        {
        }

        public override IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            Dictionary<string, object> data = ReconstructFromMultipart();
            List<InputModel> inputModels = new List<InputModel>();

            if (securityModel == null)
            {
                if (_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
                {
                    securityModel = new SecurityModel(_HttpContext);
                }
                else
                {
                    securityModel = new SecurityModelMock(_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection);
                }
            }
            InputModelParameters inputModelParameters = CreateBaseInputModelParameters(_KraftGlobalConfigurationSettings, securityModel);
            inputModelParameters = ExtendInputModelParameters(inputModelParameters);
            inputModelParameters.LoaderType |= ELoaderType.DataLoader; //TODO Not override passed in flags

            inputModelParameters.Data = data;
            

            _ProcessingContextCollection = new ProcessingContextCollection(CreateProcessingContexts(new List<InputModel>() { new InputModel(inputModelParameters) }));
            return _ProcessingContextCollection;
        }

        

        #region decoding the multipart to generic data

        public class _Entry {
            public _Entry() {
                Strings = new List<string>();
                Files = new List<IFormFile>();
            }
            public _Entry(StringValues strings):this() {
                Strings.AddRange(strings);
            }
            public _Entry(IFormFile file):this() {
                Files.Add(file);
            }
            public string Key {get; set; }
            public List<string> Strings { get; set;}
            public List<IFormFile> Files { get; set;}
            private object ResolveStringValue(string v) {
                if (!String.IsNullOrWhiteSpace(v)) {
                    if (v.Length > 1 && v.StartsWith("'") && v.EndsWith("'")) {
                        return v.Substring(1, v.Length - 2);
                    } else if (int.TryParse(v,out int i)) {
                        return i;
                    } else if (double.TryParse(v, out double d)) {
                        return d;
                    } else if (bool.TryParse(v, out bool b)) {
                        return b;
                    } else {
                        return v;
                    }
                }
                return v;
            }
            /// <summary>
            /// Gets the content as a value suitable for the internal structure.
            /// Typically single string or a single file should exist under unique name chain, but
            /// non-standard encoding on the client can pass more and we will not reject it.
            /// </summary>
            /// <value></value>
            public object Value {
                get {
                    int n = 0;
                    n += Strings.Count;
                    n += Files.Count;
                    if (n == 1) {
                        if (Strings.Count > 0) {
                            return ResolveStringValue(Strings[0]);
                        } else if (Files.Count > 0) {
                            IFormFile file = Files[0];
                            return new PostedFile(file.ContentType, file.Length, file.Name, file.FileName, f =>
                            {
                                if (f is IFormFile formFile)
                                {
                                    return formFile.OpenReadStream();
                                }
                                return null;
                            }, file);
                        } else {
                            return null;
                        }
                    } else if (n > 1) {
                        List<object> list = new List<object>();
                        if (Strings.Count > 0) {
                            list.AddRange(Strings.Select(s => ResolveStringValue(s)));
                        }
                        if (Files.Count > 0) {
                            list.AddRange(Files.Select(file => new PostedFile(file.ContentType, file.Length, file.Name, file.FileName, f =>
                            {
                                if (f is IFormFile formFile)
                                {
                                    return formFile.OpenReadStream();
                                }
                                return null;
                            }, file)));
                        }
                        return list.ToArray();
                    } else { // null
                        return null;
                    }
                }
            }
        }

        /*
            Example data representations
            JSON like source (never reaches the server)
            {
                a: 2,
                b: "3423",
                d: <file>
                c: [
                    {
                        d:  3,
                        e: "sdfs"
                        x: null,
                        y: [ ]
                    },
                    {
                        d: 5,
                        e: "fsdf",
                        x: <file>,
                        y: [ ]
                    }
                ],
                e: [
                    2,
                    "sdfsdf",
                    <file>,
                    [
                        2,
                        "sfsdf"
                    ]
                ]

                
            },
            m,ultipart rep (shortened)
            a = 2,
            b = 3424
            d = filedata
            c.1.y.0 = sdf
            c.1.y.1 = sdfsd
            c.1.e = sdfsd
            c.0.d = 3
            c.0.e = sdfsd
            c.0.x = filedata
            c.1.d = 5
            e.4.0 = 2
            c.1.x = filedata

            a.2.3.4.a


        */

        private static bool[] CalcTypes(string[] parts) {
            bool[] result = new bool[parts.Length];
            for (int i = 0; i < parts.Length; i++) {
                var part = parts[i];
                if (int.TryParse(part, out int index)) {
                    result[i] = false;
                } else {
                    result[i] = true;
                }
            }
            return result;
        }
        private static X GetCreateXinDictionary<X>(object y, string key) where X: class, new() {
            Dictionary<string, object> dict = y as Dictionary<string, object>;
            if (dict == null) {
                throw new ProcessorException<ProcessorMultipartEx>($"{key} has to be created in a dictionary, but the parent is not a dictionary.");
            }
            if (dict.ContainsKey(key)) { // Check
                if (dict[key] is X) {
                    return dict[key] as X;
                } else {
                    throw new ProcessorException<ProcessorMultipartEx>($"Data naming mix-up, the key {key} exists, but is not {typeof(X).Name}.");
                }
            } else { // Create
                dict.Add(key, new X());
                return dict[key] as X;
            }
        }
        private static X GetCreateXinArray<X>(object y, string key) where X: class, new() {
            List<object> list = y as List<object>;
            int i;
            if (list == null) {
                throw new ProcessorException<ProcessorMultipartEx>($"{key} has to be created in an array (list), but the parent is not a list.");
            }
            if (int.TryParse(key, out int index)) {
                if (list.Count > index) {
                    if (list[index] == null) { // create
                        list[index] = new X();
                        return list[index] as X;
                    } else if (list[index] is X) {
                        return list[index] as X;
                    } else { // Mixup
                        throw new ProcessorException<ProcessorMultipartEx>($"A type different than {typeof(X).Name} exists at index {index}.");
                    }
                } else { // Create and fill
                    for (i = list.Count; i < index; i++) list.Add(null);
                    list.Add(new X());
                    return list[index] as X;
                }
            } else {
                throw new Exception($"Unexpectedly cannot parse the {key} key to int.");
            }
        }
        private static void SetInStructure(Dictionary<string,object> root, string[] parts, object value) {
            bool[] types = CalcTypes(parts);
            object anchor = root;
            for (int i = 1; i < types.Length; i++) {
                if (types[i]) { // Dict to create
                    if (types[i-1]) {
                        // create dict in dict
                        anchor = GetCreateXinDictionary<Dictionary<string,object>>(anchor, parts[i]);
                    } else {
                        // create dict in array
                        anchor = GetCreateXinArray<Dictionary<string,object>>(anchor, parts[i]);
                    }
                } else { // Array to create
                    if (types[i-1]) {
                        // create array in dict
                        anchor = GetCreateXinDictionary<List<object>>(anchor, parts[i]);
                    } else {
                        // create array in array
                        anchor = GetCreateXinArray<List<object>>(anchor, parts[i]);
                    }
                }
            }
            // Set the value
            bool endtype = types[types.Length - 1];
            string part = parts[parts.Length - 1];
            if (types[types.Length - 1]) {
                if (anchor is Dictionary<string, object> dict) {
                    dict[part] = value;
                } else {
                    throw new ProcessorException<ProcessorMultipartEx>("Mixup, last part is not a dictionary");
                }
            } else {
                if (int.TryParse(part, out int index)) {
                    if (anchor is List<object> list) {
                        if (list.Count > index) {
                            list[index] = value;
                        } else {
                            for (int j = list.Count; j < index; j++) list.Add(null);
                            list.Add(value);
                        }
                    } else {
                        new ProcessorException<ProcessorMultipartEx>($"Mixup, last part {part} is not an array (list)");
                    }
                } else {
                    throw new ProcessorException<ProcessorMultipartEx>("Mixup, last part cannot be parsed as int");
                }
            }
        }

        /// <summary>
        /// See above (several methods) for a comment showing example expected content.
        /// In theory this will handle plain content as well, but it will produce just a single dictionary in that case.
        /// The main scenario assumes a javascript object to be expanded into multiple fields posted in multipart form and
        /// named with dotted chains reflecting their original position.
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, object> ReconstructFromMultipart() {
            var form = _HttpContext.Request?.Form;
            Dictionary<string, _Entry> elements = new Dictionary<string, _Entry>();
            if (form != null) {
                foreach (var kv in form) {
                    elements.Add(kv.Key,  new _Entry(kv.Value));
                }
                var files = form.Files;
                if (files != null) {
                    foreach(var ff in files) {
                        if (elements.ContainsKey(ff.Name)) {
                            elements[ff.Name].Files.Add(ff);
                        } else {
                            elements.Add(ff.Name, new _Entry(ff));
                        }
                    }
                }
            }
            var result = new Dictionary<string, object>();
            string[] parts = null;
            if (elements.Count > 0) {
                foreach (var kv in elements) {
                    _Entry entry = kv.Value;
                    parts = kv.Key.Split('.');
                    /*
                        Although the client should not typically pass arrays as values, this may happen and may be useful
                        if consumed through resolvers. So we support it - see in _Entry.Value for details.
                    */
                    SetInStructure(result, parts, entry.Value);
                }
                return result;
            } else {
                return result;
            }

        }

        #endregion


    }
}
