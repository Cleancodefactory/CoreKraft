using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Diagnostics;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ccf.Ck.Web.Middleware
{
    public class KraftExceptionHandlerMiddleware
    {
        private static Action<IApplicationBuilder> _Action = new Action<IApplicationBuilder>(HandleError);
        public static Action<IApplicationBuilder> HandleErrorAction { get => _Action; private set => _Action = value; }
        public const string EXCEPTIONSONCONFIGURE = "Configure";
        public const string EXCEPTIONSONCONFIGURESERVICES = "ConfigureServices";
        public static Dictionary<string, List<Exception>> Exceptions;
        private readonly RequestDelegate _Next;
        private readonly ExceptionHandlerOptions _Options;
        private readonly ILogger _Logger;
        private readonly Func<object, Task> _ClearCacheHeadersDelegate;
        private readonly DiagnosticSource _DiagnosticSource;

        static KraftExceptionHandlerMiddleware()
        {
            Exceptions = new Dictionary<string, List<Exception>>
            {
                { EXCEPTIONSONCONFIGURE, new List<Exception>() },
                { EXCEPTIONSONCONFIGURESERVICES, new List<Exception>() },
            };
        }
        public KraftExceptionHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, ExceptionHandlerOptions options, DiagnosticSource diagnosticSource)
        {
            _Next = next;
            _Options = options;
            _Logger = loggerFactory.CreateLogger<ExceptionHandlerMiddleware>();
            if (_Options.ExceptionHandler == null)
            {
                _Options.ExceptionHandler = _Next;
            }
            _ClearCacheHeadersDelegate = ClearCacheHeaders;
            _DiagnosticSource = diagnosticSource;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _Next(context);
                if (context.Response.StatusCode >= 400)
                {
                    string path = null;
                    if (context.Response.StatusCode == 404)
                    {
                        path = "received for: " + context.Request.Path;
                    }
                    if (!context.Response.HasStarted)
                    {
                        await WriteResponseAsync(context.Response, $"HTTP status code: {context.Response.StatusCode} {path}").ConfigureAwait(true);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(0, ex, "An unhandled exception has occurred: " + ex.Message);
                // We can't do anything if the response has already started, just abort.
                if (context.Response.HasStarted)
                {
                    _Logger.LogWarning("The response has already started, the error handler will not be executed.");
                    throw;
                }

                PathString originalPath = context.Request.Path;
                if (_Options.ExceptionHandlingPath.HasValue)
                {
                    context.Request.Path = _Options.ExceptionHandlingPath;
                }
                try
                {
                    context.Response.Clear();
                    var exceptionHandlerFeature = new ExceptionHandlerFeature()
                    {
                        Error = ex,
                        Path = originalPath.Value,
                    };
                    context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
                    context.Features.Set<IExceptionHandlerPathFeature>(exceptionHandlerFeature);
                    context.Response.StatusCode = 500;
                    context.Response.OnStarting(_ClearCacheHeadersDelegate, context.Response);

                    await _Options.ExceptionHandler(context);

                    if (_DiagnosticSource.IsEnabled("Microsoft.AspNetCore.Diagnostics.HandledException"))
                    {
                        _DiagnosticSource.Write("Microsoft.AspNetCore.Diagnostics.HandledException", new { httpContext = context, exception = ex });
                    }

                    // TODO: Optional re-throw? We'll re-throw the original exception by default if the error handler throws.
                    return;
                }
                catch (Exception ex2)
                {
                    // Suppress secondary exceptions, re-throw the original.
                    _Logger.LogError(0, ex2, "An exception was thrown attempting to execute the error handler.");
                }
                finally
                {
                    context.Request.Path = originalPath;
                }
                throw; // Re-throw the original if we couldn't handle it
            }
        }

        private Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            if (!response.HasStarted)
            {
                response.Headers[HeaderNames.CacheControl] = "no-cache";
                response.Headers[HeaderNames.Pragma] = "no-cache";
                response.Headers[HeaderNames.Expires] = "-1";
                response.Headers.Remove(HeaderNames.ETag);
            }
            return Task.CompletedTask;
        }

        private Task WriteResponseAsync(HttpResponse response, string content)
        {
            response.ContentType = "text/html";
            response.StatusCode = 200;
            var data = Encoding.UTF8.GetBytes(content);
            return response.Body.WriteAsync(data, 0, data.Length);
        }

        private static void HandleError(IApplicationBuilder builder)
        {
            if (Exceptions.Any(p => p.Value.Any()))
            {
                builder.Run(
                  async context =>
                  {
                      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                      context.Response.ContentType = "text/plain";

                      var error = context.Features.Get<IExceptionHandlerFeature>();
                      if (error != null)
                      {
                          await context.Response.WriteAsync($"<h1>Error: {error.Error.Message}</h1>").ConfigureAwait(false);
                      }
                      foreach (var ex in Exceptions)
                      {
                          foreach (var val in ex.Value)
                          {
                              await context.Response.WriteAsync($"Error on {ex.Key}: {val.Message} {Environment.NewLine} {GetErrorMessages().ToString()}").ConfigureAwait(false);
                          }
                      }
                  });
                return;
            }
        }

        private static StringBuilder GetErrorMessages()
        {
            StringBuilder sb = new StringBuilder();
            if (Exceptions.Any(p => p.Value.Any()))
            {
                foreach (var ex in Exceptions)
                {
                    foreach (var val in ex.Value)
                    {
                        sb.Append($"Error on {ex.Key}: {val.Message}");
                    }
                }
            }
            return sb;
        }
    }
}
