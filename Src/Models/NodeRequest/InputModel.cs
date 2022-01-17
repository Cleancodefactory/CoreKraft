using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Libs.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ccf.Ck.Models.Settings;

namespace Ccf.Ck.Models.NodeRequest
{
    public class InputModel
    {
        public InputModel(InputModelParameters parameters)
        {
            Module = parameters.Module;
            NodeSet = parameters.Nodeset;
            Nodepath = parameters.Nodepath;
            BindingKey = parameters.BindingKey;
            IsWriteOperation = parameters.IsWriteOperation;
            LoaderType = parameters.LoaderType;
            KraftGlobalConfigurationSettings = parameters.KraftGlobalConfigurationSettings;
            Client = new ReadOnlyDictionary<string, object>(UpdateParameters(parameters));
            Server = new ReadOnlyDictionary<string, object>(parameters.ServerVariables ?? new Dictionary<string, object>());
            SecurityModel = parameters.SecurityModel;
            Data = new ReadOnlyDictionary<string, object>(parameters.Data);
        }

        public KraftGlobalConfigurationSettings KraftGlobalConfigurationSettings { get; private set; }

        //public InputModelParameters InputModelParameters { get; private set; }

        public ReadOnlyDictionary<string, object> Client { get; private set; }

        public ReadOnlyDictionary<string, object> Server { get; private set; }

        public ISecurityModel SecurityModel { get; private set; }

        public ReadOnlyDictionary<string, object> Data { get; private set; }

        public bool IsWriteOperation { get; private set; }

        public string NodeSet { get; private set; }

        public string Nodepath { get; private set; }

        public string Module { get; private set; }

        public string BindingKey { get; private set; }

        public ELoaderType LoaderType { get; private set; }

        public object ProcessingContextRef { get; set; }

        private Dictionary<string, object> UpdateParameters(InputModelParameters parameters)
        {
            //Mix all parameter's collections
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> headerItem in parameters.HeaderCollection ?? new Dictionary<string, object>())
            {
                if (!result.ContainsKey(headerItem.Key))
                {
                    result.Add(headerItem.Key, headerItem.Value);
                }
                else
                {
                    KraftLogger.LogWarning($"Parameter from header-collection {headerItem.Key} with value {headerItem.Value} already exist in the collection and cannot be added.");
                }                
            }
            //foreach (KeyValuePair<string, object> formItem in parameters.FormCollection ?? new Dictionary<string, object>())
            //{
            //    if (!result.ContainsKey(formItem.Key))
            //    {
            //        result.Add(formItem.Key, formItem.Value);
            //    }
            //    else
            //    {
            //        KraftLogger.LogWarning($"Parameter from form-body {formItem.Key} with value {formItem.Value} already exist in the collection and cannot be added.");
            //    }                
            //}
            foreach (KeyValuePair<string, object> queryItem in parameters.QueryCollection ?? new Dictionary<string, object>())
            {
                if (!result.ContainsKey(queryItem.Key))
                {
                    result.Add(queryItem.Key, queryItem.Value);
                }
                else
                {
                    KraftLogger.LogWarning($"Parameter from query-string {queryItem.Key} with value {queryItem.Value} already exist in the collection and cannot be added.");
                }                
            }
            return result;
        }
    }
}
