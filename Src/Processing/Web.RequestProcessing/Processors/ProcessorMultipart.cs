using System;
using System.Collections.Generic;
using System.Linq;
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
using Ccf.Ck.Models.Interfaces;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorMultipart : ProcessorNodeBase
    {
        public ProcessorMultipart(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, INodeSetService nodeSetService, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) : base(httpContext, kraftModuleCollection, requestContentType, nodeSetService, kraftGlobalConfigurationSettings)
        {
        }

        public override IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            Dictionary<string, object> files = ResolveMultipartAsJson();
            List<InputModel> inputModels = new List<InputModel>();

            if (files != null)
            {
                foreach (string key in files.Keys)
                {
                    if (files[key] is IPostedFile postedFile)
                    {
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
                        inputModelParameters.ServerVariables.Add(CallTypeConstants.REQUEST_PROCESSOR, "Multipart");
                        inputModelParameters.LoaderType |= ELoaderType.DataLoader; //TODO Not override passed in flags

                        foreach (string metaInfoKey in postedFile.MetaInfo.Keys)
                        {
                            inputModelParameters.Data.Add(metaInfoKey, postedFile.MetaInfo[metaInfoKey]);
                        }
                        inputModelParameters.Data.Add(key, postedFile);

                        inputModels.Add(new InputModel(inputModelParameters, _KraftModuleCollection));
                    }
                }
            }
            _ProcessingContextCollection = new ProcessingContextCollection(CreateProcessingContexts(inputModels));
            return _ProcessingContextCollection;
        }

        public override void GenerateResponse()
        {
            IProcessingContext processingContext = new ProcessingContext(this);
            List<Dictionary<string, object>> listData = new List<Dictionary<string, object>>();
            Type postedFileType = typeof(PostedFile);

            foreach (IProcessingContext item in _ProcessingContextCollection.ProcessingContexts)
            {
                if (item.ReturnModel.Status.IsSuccessful)
                {
                    listData.Add(new Dictionary<string, object>());

                    Dictionary<string, object> currentData = item.ReturnModel.Data as Dictionary<string, object>;

                    if (currentData != null)
                    {
                        foreach (string key in currentData.Keys)
                        {
                            if (currentData[key] != null && currentData[key].GetType() != postedFileType)
                            {
                                listData[listData.Count - 1].Add(key, currentData[key]);
                            }
                        }
                    }
                }

                if (processingContext.ReturnModel.Status.IsSuccessful)
                {
                    processingContext.ReturnModel.Status.IsSuccessful = item.ReturnModel.Status.IsSuccessful;
                }

                processingContext.ReturnModel.Status.StatusResults.AddRange(item.ReturnModel.Status.StatusResults);
            }

            processingContext.ReturnModel.Data = listData;
            List<IProcessingContext> processingContexts = new List<IProcessingContext>();
            processingContexts.Add(processingContext);
            ProcessingContextCollection processingContextCollection = new ProcessingContextCollection(processingContexts);
            HttpResponseBuilder responseBuilder = new XmlPacketResponseBuilder(processingContextCollection);
            responseBuilder.GenerateResponse(_HttpContext);
        }

        protected Dictionary<string, object> ResolveMultipartAsJson()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (_HttpContext.Request.HasFormContentType)
            {
                //result = _HttpContext.Request?.Form?.Convert2Dictionary();
                Dictionary<string, object> metaInfo = ResolveMultipartFormAsJson(_HttpContext.Request?.Form);
                metaInfo["state"] = "1";

                bool atLeastOneFileExist = false;
                foreach (IFormFile file in _HttpContext.Request?.Form?.Files)
                {
                    atLeastOneFileExist = true;
                    IPostedFile postedFile = new PostedFile(file.ContentType, file.Length, file.Name, file.FileName, f =>
                    {
                        if (f is IFormFile formFile)
                        {
                            return formFile.OpenReadStream();
                        }
                        return null;
                    }, file);
                    postedFile.MetaInfo = metaInfo;
                    result.Add(Guid.NewGuid().ToString(), postedFile);
                }
                if (atLeastOneFileExist)
                {
                    //result.Add("state", "1");//Mark this for insert
                }
            }
            return result;
        }

        /// <summary>
        /// Extracts all data which is not a file from a request form.
        /// </summary>
        /// <param name="collection">Request form data</param>
        /// <returns>Dictionary of type string, object</returns>
        private Dictionary<string, object> ResolveMultipartFormAsJson(IEnumerable<KeyValuePair<string, StringValues>> collection)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (collection == null)
            {
                return result;
            }

            int startIndex = 0;
            int endIndex = 0;
            string separator = ".";
            string currentKey = string.Empty;

            Dictionary<string, object> currentlyManipulatedDictionary = null;

            foreach (KeyValuePair<string, StringValues> keyValuePair in collection)
            {
                if ((endIndex = keyValuePair.Key.IndexOf(separator, startIndex, StringComparison.InvariantCultureIgnoreCase)) > -1)
                {
                    do
                    {
                        // if there are two or more neighbour points
                        if (startIndex == endIndex)
                        {
                            throw new InvalidOperationException("Invalid key.");
                        }

                        currentKey = keyValuePair.Key.Substring(startIndex, endIndex - startIndex);

                        if (string.IsNullOrWhiteSpace(currentKey))
                        {
                            throw new InvalidOperationException("Key cannot be null, empty or white space.");
                        }

                        if (currentlyManipulatedDictionary != null)
                        {
                            currentlyManipulatedDictionary = GetOrCreateDictionary(currentlyManipulatedDictionary, currentKey);
                        }
                        else
                        {
                            currentlyManipulatedDictionary = GetOrCreateDictionary(result, currentKey);
                        }

                        startIndex = endIndex + 1;

                        endIndex = keyValuePair.Key.IndexOf(separator, startIndex, StringComparison.InvariantCultureIgnoreCase);
                    }
                    while (endIndex > -1);

                    currentKey = keyValuePair.Key.Substring(startIndex, keyValuePair.Key.Length - startIndex);

                    if (string.IsNullOrWhiteSpace(currentKey))
                    {
                        throw new InvalidOperationException("Key cannot be null, empty or white space.");
                    }

                    if (currentlyManipulatedDictionary.ContainsKey(currentKey))
                    {
                        throw new InvalidOperationException($"Key with name {currentKey} is dublicated.");
                    }

                    currentlyManipulatedDictionary.Add(currentKey, keyValuePair.Value.FirstOrDefault());

                    currentlyManipulatedDictionary = null;

                    startIndex = 0;
                    endIndex = 0;
                }
                else
                {
                    if (result.ContainsKey(keyValuePair.Key))
                    {
                        throw new InvalidOperationException($"Key with name {keyValuePair.Key} is dublicated.");
                    }

                    result.Add(keyValuePair.Key, keyValuePair.Value.FirstOrDefault());
                }
            }

            return result;
        }

        /// <summary>
        /// Creates or gets already existing dictionary in a given dicionary by a given key.
        /// </summary>
        /// <param name="parent">The dictionary which should contains the required dictionary.</param>
        /// <param name="key">The identifier of the child dictionary</param>
        /// <returns>Dictionary of string, object</returns>
        private static Dictionary<string, object> GetOrCreateDictionary(Dictionary<string, object> parent, string key)
        {
            if (parent == null)
            {
                throw new ArgumentNullException($"{nameof(Dictionary<string, object>)} cannot be null.");
            }

            if (parent.TryGetValue(key, out object childObject))
            {
                if (childObject is Dictionary<string, object>)
                {
                    return childObject as Dictionary<string, object>;
                }
                else
                {
                    throw new InvalidOperationException($"Key with name {key} is dublicated.");
                }
            }
            else
            {
                Dictionary<string, object> child = new Dictionary<string, object>();
                parent.Add(key, child);

                return child;
            }
        }
    }
}
