<<<<<<< HEAD
﻿using Ccf.Ck.Libs.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ccf.Ck.Processing.Web.Request.Utilities
{
    public static class ExtensionMethods
    {
        private const string _RequestedWithHeader = "X-Requested-With";
        private const string _XmlHttpRequest = "XMLHttpRequest";

        public static Dictionary<string, object> Convert2Dictionary(this IHeaderDictionary headerDictionary)
        {
            return Convert(headerDictionary);
        }

        public static Dictionary<string, object> Convert2Dictionary(this IQueryCollection queryCollection)
        {
            return Convert(queryCollection);
        }

        public static Dictionary<string, object> Convert2Dictionary(this IFormCollection formCollection)
        {
            return Convert(formCollection);
        }

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Headers != null)
            {
                return request.Headers[_RequestedWithHeader] == _XmlHttpRequest;
            }

            return false;
        }

        private static Dictionary<string, object> Convert(IEnumerable<KeyValuePair<string, StringValues>> collection)
        {
            //TODO see how to treat different param values with the same key
            return new Dictionary<string, object>(collection?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value.FirstOrDefault()));
        }

        public static void KraftResult(HttpContext httpContext, HttpStatusCode statusCode, string error = null)
        {
            httpContext.Response.StatusCode = (int)statusCode;
            switch (statusCode)
            {
                case HttpStatusCode.InternalServerError:
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            KraftLogger.LogCritical(error);
                        }
                        httpContext.Request.Headers.Clear();
                        break;
                    }
                case HttpStatusCode.Unauthorized:
                    {
                        //HttpRequest request = httpContext.Request;
                        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        //string redirectUrl = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), request.PathBase.ToUriComponent(), request.Path.ToUriComponent(), request.QueryString.ToUriComponent());
                        //httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = redirectUrl });
                        break;
                    }
                default:
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            KraftLogger.LogError(error);
                        }
                        httpContext.Request.Headers.Clear();
                        break;
                    }
            }
        }
    }
}
=======
﻿using Ccf.Ck.Libs.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ccf.Ck.Processing.Web.Request.Utilities
{
    public static class ExtensionMethods
    {
        private const string _RequestedWithHeader = "X-Requested-With";
        private const string _XmlHttpRequest = "XMLHttpRequest";

        public static Dictionary<string, object> Convert2Dictionary(this IHeaderDictionary headerDictionary)
        {
            return Convert(headerDictionary);
        }

        public static Dictionary<string, object> Convert2Dictionary(this IQueryCollection queryCollection)
        {
            return Convert(queryCollection);
        }

        public static Dictionary<string, object> Convert2Dictionary(this IFormCollection formCollection)
        {
            return Convert(formCollection);
        }

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Headers != null)
            {
                return request.Headers[_RequestedWithHeader] == _XmlHttpRequest;
            }

            return false;
        }

        private static Dictionary<string, object> Convert(IEnumerable<KeyValuePair<string, StringValues>> collection)
        {
            //TODO see how to treat different param values with the same key
            return new Dictionary<string, object>(collection?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value.FirstOrDefault()));
        }

        public static void KraftResult(HttpContext httpContext, HttpStatusCode statusCode, string error = null)
        {
            httpContext.Response.StatusCode = (int)statusCode;
            switch (statusCode)
            {
                case HttpStatusCode.InternalServerError:
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            KraftLogger.LogCritical(error);
                        }
                        httpContext.Request.Headers.Clear();
                        break;
                    }
                case HttpStatusCode.Unauthorized:
                    {
                        //HttpRequest request = httpContext.Request;
                        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        //string redirectUrl = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), request.PathBase.ToUriComponent(), request.Path.ToUriComponent(), request.QueryString.ToUriComponent());
                        //httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = redirectUrl });
                        break;
                    }
                default:
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            KraftLogger.LogError(error);
                        }
                        httpContext.Request.Headers.Clear();
                        break;
                    }
            }
        }
    }
}
>>>>>>> develop
