using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Data.Call.Models;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Data.Call
{
    /// <summary>
    /// System plugin which executes other system and node plugins. 
    /// The called plugins will be executed in the same operation (read or write) as the operation of the CallDataLoader plugin. 
    /// </summary>
    public class CallDataLoaderImp : DataLoaderClassicBase<CallDataLoaderSynchronizeContextScopedImp>
    {
        private const string STATENAME = "state";

        /// <summary>
        /// Calls and executes plugin in read operation.
        /// </summary>
        /// <param name="execContext">Data loader context.</param>
        /// <returns>Called plugin result.</returns>
        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            Dictionary<string, object> parameters = ParseAdditionalParameters(execContext.CurrentNode.Read);

            return ExecuteOperation(execContext, parameters, false);
        }

        /// <summary>
        /// Calls and execute plugin in write operation.
        /// </summary>
        /// <param name="execContext">Data loader context.</param>
        /// <returns>Called plugin result.</returns>
        protected override object Write(IDataLoaderWriteContext execContext)
        {
            EDataState state = EDataState.Unchanged;

            if (execContext.Row.ContainsKey(STATENAME))
            {
                if (int.TryParse(execContext.Row[STATENAME].ToString(), out int stateAsInt))
                {
                    state = (EDataState)stateAsInt;
                }
            }

            Dictionary<string, object> parameters = ParseAdditionalParameters(execContext.CurrentNode.Write, state);

            return ExecuteOperation(execContext, parameters, true);
        }

        /// <summary>
        /// Calls and executes plugin.
        /// </summary>
        /// <param name="execContext">Data loader context.</param>
        /// <param name="parameters">Dictionary or custom parameters of the Call Data Loader.</param>
        /// <param name="isWriteOperation">boolean parameter - the type of operation.</param>
        /// <returns>The called plugin result.</returns>
        private List<Dictionary<string, object>> ExecuteOperation(IDataLoaderContext execContext, Dictionary<string, object> parameters, bool isWriteOperation)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            CustomSettings customSettings = new CustomSettings(execContext, isWriteOperation);

            if (execContext.ProcessingContext.InputModel.ProcessingContextRef is RequestExecutor requestExecutor)
            {
                parameters = ConcatDictionaries(parameters, GetChildrenFromKeyRecursive(execContext.ProcessingContext.InputModel.Data, execContext.CurrentNode.NodeKey) as IDictionary<string, object>);

                object getChildren = GetChildrenFromKeyRecursive(execContext.ProcessingContext.InputModel.Data, execContext.CurrentNode.NodeKey);

                if (getChildren != null && getChildren is IDictionary<string, object> children)
                {
                    parameters = ConcatDictionaries(parameters, children);
                }
                else
                {
                    parameters = ConcatDictionaries(parameters, execContext.ProcessingContext.InputModel.Data);

                    KraftLogger.LogDebug($"Key '{execContext.CurrentNode.NodeKey}' was not passed in the request data. The CallDataLoader will be executed with input model's data. For the request to node '{execContext.ProcessingContext.InputModel.Module}.{execContext.ProcessingContext.InputModel.NodeSet}.{execContext.CurrentNode.NodeKey}'.");
                }

                InputModelParameters inputModelParameters = new InputModelParameters()
                {
                    Module = customSettings.ModuleValue,
                    Nodeset = customSettings.NodesetValue,
                    Nodepath = customSettings.NodepathValue,
                    Data = parameters,
                    FormCollection = execContext.ParentResult,
                    KraftGlobalConfigurationSettings = execContext.ProcessingContext.InputModel.KraftGlobalConfigurationSettings,
                    IsWriteOperation = customSettings.OperationValue,
                    LoaderType = execContext.ProcessingContext.InputModel.LoaderType,
                    SecurityModel = execContext.ProcessingContext.InputModel.SecurityModel,
                    ServerVariables = execContext.ProcessingContext.InputModel.Server != default(ReadOnlyDictionary<string, object>) ? execContext.ProcessingContext.InputModel.Server.ToDictionary(item => item.Key, item => item.Value) : null
                };

                IProcessingContext processingContext = new ProcessingContext(execContext.ProcessingContext.ProcessorHandler)
                {
                    InputModel = new InputModel(inputModelParameters)
                };

                requestExecutor.ExecuteReEntrance(processingContext, false);

                if (!processingContext.ReturnModel.Status.IsSuccessful)
                {
                    string message = string.Empty;
                    string space = " ";

                    execContext.ProcessingContext.ReturnModel.Status.IsSuccessful = processingContext.ReturnModel.Status.IsSuccessful;

                    processingContext.ReturnModel.Status.StatusResults.ForEach(statusResult =>
                    {
                        if (message.Length != 0)
                        {
                            message += space + statusResult.Message;
                        }
                        else
                        {
                            message += statusResult.Message;
                        }
                    });

                    throw new Exception(message);
                }

                if (processingContext.ReturnModel.Data is List<Dictionary<string, object>> resultListOfDictionary)
                {
                    result = resultListOfDictionary;
                }
                else if (processingContext.ReturnModel.Data is Dictionary<string, object> resultDictionary)
                {
                    result.Add(resultDictionary);
                }
            }

            return result;
        }

        private object GetChildrenFromKeyRecursive(IDictionary<string, object> data, string key)
        {
            foreach (KeyValuePair<string, object> item in data)
            {
                if (item.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    return item.Value;
                }
                else
                {
                    if (item.Value is IDictionary<string, object> dict)
                    {
                        return GetChildrenFromKeyRecursive(dict, key);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Parses custom parameters passed in different operation (for example {"read" : { "query": "key1=value1&key2=value2" } }).
        /// </summary>
        /// <param name="operation">Operation base - read, write.</param>
        /// <param name="state">Unchange, Insert, Update or Delete.</param>
        /// <returns>Returns dictionary of parsed values.</returns>
        private Dictionary<string,object> ParseAdditionalParameters(OperationBase operation, EDataState state = EDataState.Unchanged)
        {
            Dictionary<string, object> resultDictionary = new Dictionary<string, object>();
            string parseText = null;

            if (operation is Read read)
            {
                parseText = read?.Select?.Query ?? null;
            }

            if (operation is Write write)
            {
                if (state == EDataState.Deleted && (write?.Delete?.Query ?? null) != null)
                {
                    parseText = write.Delete.Query;
                }

                if (state == EDataState.Insert && (write?.Insert?.Query ?? null) != null)
                {
                    parseText = write.Insert.Query;
                }

                if (state == EDataState.Deleted && (write?.Update?.Query ?? null) != null)
                {
                    parseText = write.Update.Query;
                }
            }

            if (string.IsNullOrWhiteSpace(parseText))
            {
                return resultDictionary;
            }

            string[] keyValues = parseText.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string keyValue in keyValues)
            {
                string[] values = keyValue.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Length == 2)
                {
                    if (resultDictionary.ContainsKey(values[0]))
                    {
                        resultDictionary[values[0]] = values[1];
                    }
                    else
                    {
                        resultDictionary.Add(values[0], values[1]);
                    }
                }
            }

            return resultDictionary;
        }

        /// <summary>
        /// Concats two dictionaries.
        /// </summary>
        /// <param name="firstDictionary">First dictionary to concat.</param>
        /// <param name="secondDictionary">Second ditionary to concat.</param>
        /// <returns>Concat dictionary from firstDictionary and secondDictionary</returns>
        private Dictionary<string, object> ConcatDictionaries(Dictionary<string, object> firstDictionary, IDictionary<string, object> secondDictionary)
        {
            if (firstDictionary == null || secondDictionary == null)
            {
                return firstDictionary;
            }

            foreach (string key in secondDictionary.Keys)
            {
                if (!firstDictionary.ContainsKey(key))
                {
                    firstDictionary.Add(key, secondDictionary[key]);
                }
                else
                {
                    firstDictionary[key] = secondDictionary[key];
                }
            }

            return firstDictionary;
        }
    }
}