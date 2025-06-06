﻿using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Ccf.Ck.Utilities.NodeSetService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;
using static Ccf.Ck.Processing.Web.Request.BaseClasses.ProcessorBase;

namespace Ccf.Ck.Processing.Web.Request
{

    internal class KraftProcessorFactory : AbstractProcessorFactory
    {
        const long MAX_REWINDABLE_SIZE = 1000000; //1MB
        private const string CONTENTTYPEFIRSTPART = @"(?<firstpart>.*?(?=;|$|\s))";
        private static Regex _ContentTypeFirstPartRegex = new Regex(CONTENTTYPEFIRSTPART, RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        internal override ProcessorBase CreateProcessor(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, INodeSetService nodesSetService, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            RouteData routeData = httpContext.GetRouteData();
            //see class: KraftRouteBuilder
            //In KraftRouteBuilder all routings are defined
            ESupportedContentTypes contentType = MapContentType(httpContext);
            
            if (routeData.Values != null)
            {
                string routeDataKey = routeData.DataTokens["key"]?.ToString()?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(routeDataKey))
                {
                    switch (routeDataKey)
                    {
                        case Constants.RouteSegmentConstants.RouteDataTokenWarmup:
                            {
                                return new ProcessorWarmup(httpContext, kraftModuleCollection, contentType, kraftGlobalConfigurationSettings);
                            }
                        case Constants.RouteSegmentConstants.RouteDataTokenSignal:
                        case Constants.RouteSegmentConstants.RouteDataTokenSignalRead:
                        case Constants.RouteSegmentConstants.RouteDataTokenSignalWrite:
                            {
                                return new ProcessorSignal(httpContext, kraftModuleCollection, contentType, nodesSetService, kraftGlobalConfigurationSettings);
                            }
                        case Constants.RouteSegmentConstants.RouteDataTokenView:
                            {
                                return new ProcessorView(httpContext, kraftModuleCollection, contentType, nodesSetService, kraftGlobalConfigurationSettings);
                            }
                        //case Constants.RouteSegmentConstants.RouteDataTokenResource:
                        //    {
                        //        //return new ProcessorResource(httpContext, kraftModuleCollection);
                        //        break;
                        //    }
                        case Constants.RouteSegmentConstants.RouteDataTokenBatch:
                            {
                                return new ProcessorNodeBatch(httpContext, kraftModuleCollection, contentType, nodesSetService, kraftGlobalConfigurationSettings);
                            }
                        default: // Processes read, write, single
                            {
                                //Here we have the CoreKraft configured entry point
                                switch (contentType)
                                {
                                    case ESupportedContentTypes.JSON:
                                    case ESupportedContentTypes.FORM_URLENCODED:
                                        {
                                            bool preserveBody = false;
                                            if (!string.IsNullOrEmpty(kraftGlobalConfigurationSettings.GeneralSettings.EnableBufferQueryParameter))
                                            {
                                                if (httpContext.Request.Query.ContainsKey(kraftGlobalConfigurationSettings.GeneralSettings.EnableBufferQueryParameter))
                                                {
                                                    if (httpContext.Request.ContentLength < MAX_REWINDABLE_SIZE)
                                                    {
                                                        preserveBody = true;
                                                    }
                                                }
                                            }
                                            
                                            return new ProcessorNodeSingle(httpContext, 
                                                            kraftModuleCollection, 
                                                            contentType, 
                                                            nodesSetService, 
                                                            kraftGlobalConfigurationSettings, 
                                                            preserveBody);
                                        }
                                    case ESupportedContentTypes.FORM_MULTIPART:
                                        {
                                            if (httpContext.Request.Headers.ContainsKey("JSONLike-Multipart")) {
                                                return new ProcessorMultipartEx(httpContext, kraftModuleCollection, contentType, nodesSetService, kraftGlobalConfigurationSettings);
                                            } else {
                                                return new ProcessorMultipart(httpContext, kraftModuleCollection, contentType, nodesSetService, kraftGlobalConfigurationSettings);
                                            }
                                        }
                                    default:
                                        {
                                            return new ProcessorUnknown(httpContext, kraftModuleCollection, contentType, kraftGlobalConfigurationSettings);
                                        }
                                }
                            }                            
                    }
                }
            }
            return new ProcessorUnknown(httpContext, kraftModuleCollection, contentType, kraftGlobalConfigurationSettings);
        }

        private ESupportedContentTypes MapContentType(HttpContext httpContext)
        {
            const string CONTENTTYPEJSON = "application/json";
            const string CONTENTTYPEMULTIPART = "multipart/form-data";
            const string CONTENTTYPEFORM = "application/x-www-form-urlencoded";
            string contentType = httpContext?.Request?.ContentType;
            if (string.IsNullOrEmpty(contentType))//Right now BindKraft has no content type for GET requests
            {
                return ESupportedContentTypes.JSON;//Default when empty
            }
            Match match = _ContentTypeFirstPartRegex.Match(contentType);
            if (match.Success)
            {
                contentType = match.Groups["firstpart"].ToString();
                switch (contentType)
                {
                    case CONTENTTYPEJSON:
                        {
                            return ESupportedContentTypes.JSON;
                        }
                    case CONTENTTYPEMULTIPART:
                        {
                            return ESupportedContentTypes.FORM_MULTIPART;
                        }
                    case CONTENTTYPEFORM:
                        {
                            return ESupportedContentTypes.FORM_URLENCODED;
                        }
                    default:
                        {
                            //we are trying to find if a file/binary is in the body
                            if (httpContext.Request.ContentLength == null && httpContext.Request.Headers["Transfer-Encoding"] == "chunked")
                            {
                                return ESupportedContentTypes.FORM_MULTIPART;
                            }
                            break;
                        }
                }
            }
            return ESupportedContentTypes.UNKNOWN;
        }
    }
}
