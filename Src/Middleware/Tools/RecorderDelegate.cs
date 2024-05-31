using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Recorders.Store;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Ccf.Ck.Web.Middleware.Tools
{
    internal class RecorderDelegate
    {
        internal static RequestDelegate ExecutionDelegate(IApplicationBuilder app, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            RequestDelegate requestDelegate = async httpContext =>
            {
                httpContext.Request.RouteValues.TryGetValue("p", out object val);
                ISecurityModel securityModel = new SecurityModelMock(kraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection);
                const string contentType = "application/json";
                int statusCode = (int)HttpStatusCode.OK;
                JsonMessage jsonMessage = new JsonMessage();
                if (kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.IsEnabled)
                {
                    if (kraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
                    {
                        securityModel = new SecurityModel(httpContext);
                    }
                }
                switch (val)
                {
                    case "0": //Pause
                        {
                            if (kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.IsEnabled)
                            {
                                if (securityModel.IsAuthenticated)
                                {
                                    RecordersStoreImp recordersStoreImp = app.ApplicationServices.GetRequiredService<RecordersStoreImp>();
                                    IRequestRecorder requestRecorder = recordersStoreImp.Get(securityModel.UserName);
                                    if (requestRecorder != null)
                                    {
                                        requestRecorder.IsRunning = false;
                                        jsonMessage.Message = "Recorder is paused.";
                                    }
                                    else
                                    {
                                        jsonMessage.Message = "The Recorder is not running.";
                                    }
                                    jsonMessage.Success = true;
                                    statusCode = (int)HttpStatusCode.OK;
                                }
                                else
                                {
                                    jsonMessage.Message = "Please login because the recorder can't be run for anonymous users.";
                                    jsonMessage.Success = false;
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                jsonMessage.Message = "Recorder is not configured and can't be started.";
                                jsonMessage.Success = false;
                                statusCode = (int)HttpStatusCode.NotFound;
                            }
                            break;
                        }
                    case "1"://Start
                        {
                            if (kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.IsEnabled)
                            {
                                if (securityModel.IsAuthenticated)
                                {
                                    Type typeRecorder = Type.GetType(kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.ImplementationAsString, true);
                                    IRequestRecorder requestRecorder = Activator.CreateInstance(typeRecorder) as IRequestRecorder;
                                    RecordersStoreImp recordersStoreImp = app.ApplicationServices.GetRequiredService<RecordersStoreImp>();
                                    recordersStoreImp.Set(requestRecorder, securityModel.UserName);
                                    requestRecorder.IsRunning = true;
                                    jsonMessage.Message = "Recorder is enabled";
                                    jsonMessage.Success = true;
                                    statusCode = (int)HttpStatusCode.OK;
                                }
                                else
                                {
                                    jsonMessage.Message = "Please login because the recorder can't be run for anonymous users.";
                                    jsonMessage.Success = false;
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                jsonMessage.Message = "Recorder is not configured and can't be started.";
                                jsonMessage.Success = false;
                                statusCode = (int)HttpStatusCode.NotFound;
                            }
                            break;
                        }
                    case "2"://Download
                        {
                            if (kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.IsEnabled)
                            {
                                if (securityModel.IsAuthenticated)
                                {
                                    RecordersStoreImp recordersStoreImp = app.ApplicationServices.GetRequiredService<RecordersStoreImp>();
                                    IRequestRecorder requestRecorder = recordersStoreImp.Get(securityModel.UserName);
                                    if (requestRecorder != null)
                                    {
                                        jsonMessage.Message = requestRecorder.GetFinalResult()?.Result ?? string.Empty;
                                        jsonMessage.Success = true;
                                        Type typeRecorder = Type.GetType(kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.ImplementationAsString, true);
                                        requestRecorder = Activator.CreateInstance(typeRecorder) as IRequestRecorder;
                                        recordersStoreImp.Set(requestRecorder, securityModel.UserName);
                                        httpContext.Response.Clear();
                                        //Don't calculate the response length!
                                        //httpContext.Response.Headers.Add("Content-Length", message.Length.ToString());
                                        httpContext.Response.Headers["Content-Disposition"] = "attachment;filename=RecordedSession.json";
                                        statusCode = (int)HttpStatusCode.OK;
                                        httpContext.Response.StatusCode = statusCode;
                                        httpContext.Response.ContentType = "text/html; charset=UTF-8";
                                        await httpContext.Response.WriteAsync(jsonMessage.Message, Encoding.UTF8);
                                        return;
                                    }
                                    else
                                    {
                                        jsonMessage.Message = "The Recorder is not enabled and no data is available.";
                                        jsonMessage.Success = false;
                                        statusCode = (int)HttpStatusCode.OK;
                                    }
                                }
                                else
                                {
                                    jsonMessage.Message = "Please login because the recorder can't be run for anonymous users.";
                                    jsonMessage.Success = false;
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                jsonMessage.Message = "Recorder is not configured and can't be started.";
                                jsonMessage.Success = false;
                                statusCode = (int)HttpStatusCode.NotFound;
                            }
                            break;
                        }
                    case "3"://Delete information
                        {
                            if (kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.IsEnabled)
                            {
                                if (securityModel.IsAuthenticated)
                                {
                                    RecordersStoreImp recordersStoreImp = app.ApplicationServices.GetRequiredService<RecordersStoreImp>();
                                    IRequestRecorder requestRecorder = recordersStoreImp.Get(securityModel.UserName);
                                    if (requestRecorder != null)
                                    {
                                        recordersStoreImp.Remove(securityModel.UserName);
                                        jsonMessage.Message = "Recorder is destroyed.";
                                    }
                                    else
                                    {
                                        jsonMessage.Message = "The Recorder is not running.";
                                    }
                                    jsonMessage.Success = true;
                                    statusCode = (int)HttpStatusCode.OK;
                                }
                                else
                                {
                                    jsonMessage.Message = "Please login because the recorder can't be used for anonymous users.";
                                    jsonMessage.Success = false;
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                jsonMessage.Message = "Recorder is not configured and can't be started.";
                                jsonMessage.Success = false;
                                statusCode = (int)HttpStatusCode.NotFound;
                            }
                            break;
                        }
                    case "4": //Check
                        if (kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.IsEnabled)
                        {
                            if (securityModel.IsAuthenticated)
                            {
                                jsonMessage.Message = "Recorder is enabled and you are all set up.";
                                jsonMessage.Success = true;
                                statusCode = (int)HttpStatusCode.OK;
                            }
                            else
                            {
                                jsonMessage.Message = "Please login and try again.";
                                jsonMessage.Success = false;
                                statusCode = (int)HttpStatusCode.Unauthorized;
                            }
                        }
                        else
                        {
                            jsonMessage.Message = "Recorder is not configured and can't be started.";
                            jsonMessage.Success = false;
                            statusCode = (int)HttpStatusCode.NotFound;
                        }
                        break;
                    default:
                        break;
                }
                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.ContentType = contentType;
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(jsonMessage), Encoding.UTF8);
            };
            return requestDelegate;
        }
    }
}
