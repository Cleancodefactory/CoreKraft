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
                int statusCode = 200;
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
                    case "0":
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
                                }
                                else
                                {
                                    message = "Please login because the recorder can't be run for anonymous users.";
                                }
                            }
                            else
                            {
                                statusCode = (int)HttpStatusCode.NotFound;
                                message = "Recorder is not configured and can't be started.";
                            }
                            break;
                        }
                    case "1":
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
                                }
                                else
                                {
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                    message = "Please login because the recorder can't be run for anonymous users.";
                                }
                            }
                            else
                            {
                                statusCode = (int)HttpStatusCode.NotFound;
                                message = "Recorder is not configured and can't be started.";
                            }
                            break;
                        }
                    case "2":
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
                                        httpContext.Response.Headers.Add("Content-Length", message.Length.ToString());
                                        httpContext.Response.Headers.Add("Content-Disposition", "attachment;filename=RecordedSession.json");
                                    }
                                    else
                                    {
                                        message = "The Recorder is not enabled and no data is available.";
                                    }
                                }
                                else
                                {
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                    message = "Please login because the recorder can't be run for anonymous users.";
                                }
                            }
                            else
                            {
                                statusCode = (int)HttpStatusCode.NotFound;
                                message = "Recorder is not configured and can't be started.";
                            }
                            break;
                        }
                    case "3":
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
                                }
                                else
                                {
                                    statusCode = (int)HttpStatusCode.Unauthorized;
                                    message = "Please login because the recorder can't be used for anonymous users.";
                                }
                            }
                            else
                            {
                                statusCode = (int)HttpStatusCode.NotFound;
                                message = "Recorder is not configured and can't be started.";
                            }
                            break;
                        }
                    default:
                        break;
                }
                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.ContentType = contentType;
                await httpContext.Response.WriteAsync(message);
            };
            return requestDelegate;
        }
    }
}
