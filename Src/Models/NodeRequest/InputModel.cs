using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ccf.Ck.Models.NodeRequest
{
    // Info about the ExtractRequestDataCallback , not strictly typed to avoid cycled refs
    //    public delegate object ExtractRequestData(string what);
    public class InputModel
    {
        public InputModel(InputModelParameters parameters, KraftModuleCollection kraftModuleCollection)
        {
            if (kraftModuleCollection != null)
            {
                Module = kraftModuleCollection.AdjustCasing(parameters.Module);
            }
            else
            {
                Module= parameters.Module;
            }
            NodeSet = parameters.Nodeset;
            Nodepath = parameters.Nodepath;
            BindingKey = parameters.BindingKey;
            IsWriteOperation = parameters.IsWriteOperation;
            LoaderType = parameters.LoaderType;
            KraftGlobalConfigurationSettings = parameters.KraftGlobalConfigurationSettings;
            Client = new ReadOnlyDictionary<string, object>(UpdateParameters(parameters));
            Server = new ReadOnlyDictionary<string, object>(parameters.ServerVariables ?? new Dictionary<string, object>());
            SecurityModel = parameters.SecurityModel;
            ReadAction = parameters.ReadAction;
            Data = new ReadOnlyDictionary<string, object>(parameters.Data);
        }

        public KraftGlobalConfigurationSettings KraftGlobalConfigurationSettings { get; private set; }

        //public InputModelParameters InputModelParameters { get; private set; }
        /// <summary>
        /// Specifies the read action. By default it is select. Not used for write operations
        /// </summary>
        public EReadAction ReadAction { get; set; } = EReadAction.Default;

        public ReadOnlyDictionary<string, object> Client { get; private set; }

        public ReadOnlyDictionary<string, object> Server { get; private set; }

        public ISecurityModel SecurityModel { get; private set; }

        public ReadOnlyDictionary<string, object> Data { get; private set; }

        public bool IsWriteOperation { get; private set; }

        public string NodeSet { get; private set; }

        public string Nodepath { get; private set; }

        public string Module { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string BindingKey { get; private set; }

        public ELoaderType LoaderType { get; private set; }

        public object ProcessingContextRef { get; set; }

        public Func<string,object> ExtractRequestDataCallback { get; set; }
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
            // Reminder that form data have bneen in the Client collection before 20.01.2022
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
