using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Utilities.NodeSetService;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorNodeBatch : ProcessorNodeBase
    {
        public ProcessorNodeBatch(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, INodeSetService nodeSetService, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) : base(httpContext, kraftModuleCollection, requestContentType, nodeSetService, kraftGlobalConfigurationSettings)
        {
        }

        public override IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            List<InputModel> inputModels = new List<InputModel>();
            List<BatchRequest> batchRequests = new List<BatchRequest>();
            if (_RequestContentType == ESupportedContentTypes.JSON)
            {
                batchRequests = GetBodyJson<List<BatchRequest>>(_HttpContext.Request);
            }
            foreach (BatchRequest batchRequest in batchRequests)
            {
                InputModel inputModel = CreateInputModel(batchRequest, _KraftGlobalConfigurationSettings, kraftRequestFlagsKey, securityModel);
                inputModels.Add(inputModel);
            }
            _ProcessingContextCollection = new ProcessingContextCollection(CreateProcessingContexts(inputModels));
            return _ProcessingContextCollection;
        }

        private InputModel CreateInputModel(BatchRequest request, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            if (securityModel == null)
            {
                if (kraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
                {
                    securityModel = new SecurityModel(_HttpContext);
                }
                else
                {
                    securityModel = new SecurityModelMock(kraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection);
                }
            }
            InputModelParameters inputModelParameters = CreateBaseInputModelParameters(kraftGlobalConfigurationSettings, securityModel);
            //we expect the routing to be like this:
            //domain.com/startnode/<read|write>/module/nodeset/<nodepath>?lang=de
            string decodedUrl = WebUtility.UrlDecode(request.Url);
            string pattern = @"(?:http:\/\/.+?\/|https:\/\/.+?\/|www.+?\/)(?:.+?)\/(read|write)*(?:\/)*(.+?)\/(.+?)($|\/.+?)($|\?.+)";
            Regex regex = new Regex(pattern);
            var match = regex.Match(decodedUrl);

            if (match.Groups[1] != null)
            {
                inputModelParameters.IsWriteOperation = match.Groups[1].Value == "write";
            }
            inputModelParameters.Module = match.Groups[2].Value;
            inputModelParameters.Nodeset = match.Groups[3].Value;
            inputModelParameters.Nodepath = string.IsNullOrEmpty(match.Groups[4].Value) ? string.Empty : match.Groups[4].Value.Substring(1);
            string paramsStr = string.IsNullOrEmpty(match.Groups[5].Value) ? string.Empty : match.Groups[5].Value.Substring(1);

            if (!string.IsNullOrEmpty(paramsStr))
            {
                string[] parameters = paramsStr.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var parameter in parameters)
                {
                    string[] kvp = parameter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    string key = kvp[0];
                    string value = kvp[1];

                    if (key.Equals(kraftRequestFlagsKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        inputModelParameters.LoaderType = GetLoaderContent(value);
                    }
                    else
                    {
                        inputModelParameters.QueryCollection.Add(key, value);
                    }
                }
            }
            return new InputModel(inputModelParameters);
        }
    }
}
