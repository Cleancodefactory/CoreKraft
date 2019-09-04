using Ccf.Ck.Models.KraftModule;
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
        private const string CONTENTTYPEFIRSTPART = @"(?<firstpart>.*?(?=;|$|\s))";
        private static Regex _ContentTypeFirstPartRegex = new Regex(CONTENTTYPEFIRSTPART, RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        internal override ProcessorBase CreateProcessor(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, INodeSetService nodesSetService)
        {
            RouteData routeData = httpContext.GetRouteData();
            //see class: KraftRouteBuilder
            //In KraftRouteBuilder all routings are defined
            ESupportedContentTypes contentType = MapContentType(httpContext);
            if (routeData.Values != null)
            {
                string routeDataKey = routeData.DataTokens["key"]?.ToString()?.ToLower();
                if (!string.IsNullOrEmpty(routeDataKey))
                {
                    switch (routeDataKey)
                    {
                        case Constants.RouteSegmentConstants.RouteDataTokenWarmup:
                            {
                                return new ProcessorWarmup(httpContext, kraftModuleCollection, contentType);
                            }
                        case Constants.RouteSegmentConstants.RouteDataTokenSignal:
                        case Constants.RouteSegmentConstants.RouteDataTokenSignalRead:
                        case Constants.RouteSegmentConstants.RouteDataTokenSignalWrite:
                            {
                                return new ProcessorSignal(httpContext, kraftModuleCollection, contentType, nodesSetService);
                            }
                        case Constants.RouteSegmentConstants.RouteDataTokenView:
                            {
                                return new ProcessorView(httpContext, kraftModuleCollection, contentType, nodesSetService);
                            }
                        //case Constants.RouteSegmentConstants.RouteDataTokenResource:
                        //    {
                        //        //return new ProcessorResource(httpContext, kraftModuleCollection);
                        //        break;
                        //    }
                        case Constants.RouteSegmentConstants.RouteDataTokenBatch:
                            {
                                return new ProcessorNodeBatch(httpContext, kraftModuleCollection, contentType, nodesSetService);
                            }
                        default:
                            {
                                //Here we have the CoreKraft configured entry point
                                switch (contentType)
                                {
                                    case ESupportedContentTypes.JSON:
                                    case ESupportedContentTypes.FORM_URLENCODED:
                                        {
                                            return new ProcessorNodeSingle(httpContext, kraftModuleCollection, contentType, nodesSetService);
                                        }
                                    case ESupportedContentTypes.FORM_MULTIPART:
                                        {
                                            return new ProcessorMultipart(httpContext, kraftModuleCollection, contentType, nodesSetService);
                                        }
                                    default:
                                        {
                                            return new ProcessorUnknown(httpContext, kraftModuleCollection, contentType);
                                        }
                                }
                            }                            
                    }
                }
            }
            return new ProcessorUnknown(httpContext, kraftModuleCollection, contentType);
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
            //we are trying to find if a file/binary is in the body
            if (httpContext.Request.ContentLength == null && httpContext.Request.Headers["Transfer-Encoding"] == "chunked")
            {
                return ESupportedContentTypes.FORM_MULTIPART;
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
                            return ESupportedContentTypes.UNKNOWN;
                        }
                }
            }
            return ESupportedContentTypes.UNKNOWN;
        }
    }
}
