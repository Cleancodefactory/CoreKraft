using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Ccf.Ck.Web.Middleware.Tools
{
    internal class ErrorsDelegate
    {
        internal static RequestDelegate ExecutionDelegate(IApplicationBuilder app, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            RequestDelegate requestDelegate = async httpContext =>
            {
                ISecurityModel securityModel = new SecurityModelMock(kraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection);
                const string contentType = "application/json";
                JsonMessage jsonMessage = new JsonMessage();
                jsonMessage.Message = "You are not allowed to retrieve errors or this setting is generally disabled.";
                jsonMessage.Success = false;
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized; ;
                httpContext.Response.ContentType = contentType;
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(jsonMessage), Encoding.UTF8);
            };
            return requestDelegate;
        }
    }
}
