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
                const string contentType = "text/html; charset=UTF-8";
                int statusCode = (int)HttpStatusCode.OK;
                string message = string.Empty;
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
                                        message = "Recorder is paused.";
                                    }
                                    else
                                    {
                                        message = "The Recorder is not running.";
                                    }
                                    statusCode = (int)HttpStatusCode.OK;
                                }
                                else
                                {
                                    message = "Please login because the recorder can't be run for anonymous users.";
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                message = "Recorder is not configured and can't be started.";
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
                                    message = "Recorder is enabled";
                                    statusCode = (int)HttpStatusCode.OK;
                                }
                                else
                                {
                                    message = "Please login because the recorder can't be run for anonymous users.";
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                message = "Recorder is not configured and can't be started.";
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
                                        message = requestRecorder.GetFinalResult()?.Result ?? string.Empty;
                                        Type typeRecorder = Type.GetType(kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.ImplementationAsString, true);
                                        requestRecorder = Activator.CreateInstance(typeRecorder) as IRequestRecorder;
                                        recordersStoreImp.Set(requestRecorder, securityModel.UserName);
                                        httpContext.Response.Clear();
                                        //Don't calculate the response length!
                                        //httpContext.Response.Headers.Add("Content-Length", message.Length.ToString());
                                        httpContext.Response.Headers.Add("Content-Disposition", "attachment;filename=RecordedSession.json");
                                        statusCode = (int)HttpStatusCode.OK;
                                    }
                                    else
                                    {
                                        message = "The Recorder is not enabled and no data is available.";
                                        statusCode = (int)HttpStatusCode.OK;
                                    }
                                }
                                else
                                {
                                    message = "Please login because the recorder can't be run for anonymous users.";
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                message = "Recorder is not configured and can't be started.";
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
                                        message = "Recorder is destroyed.";
                                    }
                                    else
                                    {
                                        message = "The Recorder is not running.";
                                    }
                                    statusCode = (int)HttpStatusCode.OK;
                                }
                                else
                                {
                                    message = "Please login because the recorder can't be used for anonymous users.";
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                message = "Recorder is not configured and can't be started.";
                                statusCode = (int)HttpStatusCode.NotFound;
                            }
                            break;
                        }
                    case "4": //Check
                        if (kraftGlobalConfigurationSettings.GeneralSettings.ToolsSettings.RequestRecorder.IsEnabled)
                        {
                            if (securityModel.IsAuthenticated)
                            {
                                statusCode = (int)HttpStatusCode.OK;
                            }
                            else
                            {
                                statusCode = (int)HttpStatusCode.Unauthorized;
                            }
                        }
                        else
                        {
                            statusCode = (int)HttpStatusCode.NotFound;
                        }
                        break;
                    default:
                        break;
                }
                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.ContentType = contentType;
                await httpContext.Response.WriteAsync(message, Encoding.UTF8);
            };
            return requestDelegate;
        }
    }
}
