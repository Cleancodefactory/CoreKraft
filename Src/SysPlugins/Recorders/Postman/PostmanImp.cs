using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Recorders.Postman.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Postman
{
    /// <summary>
    /// Responsible for generating file that holds model compatible with Postman. 
    /// To be use for generating(importing) Postman Runner Collections for automation tests of API 
    /// </summary>
    public class PostmanImp : IRequestRecorder
    {
        // Holds static information about the Authentication and Postman shema version
        // Contains array with every request passed through the API
        private PostmanRunnerModel _RunnerModel = new PostmanRunnerModel();
        public const string BASE_HOST = "{{base_host}}";
        public const string COOKIE = "{{cookie}}";
        private string _CookieValue;

        public bool IsRunning { get; set; }

        public Task<string> GetFinalResult()
        {
            if (_CookieValue != null)
            {
                _RunnerModel.UpdatePreRequestEvents(_CookieValue);
            }
            string result = GetJsonString(_RunnerModel);
            _RunnerModel = new PostmanRunnerModel();
            return Task.FromResult(result);
        }

        public async Task HandleRequest(HttpRequest request)
        {
            #region Setting up required data from the HttpRequest
            string body = await GetBodyAsync(request);
            string method = request.Method;

            List<PostmanHeaderSection> pHeaders = new List<PostmanHeaderSection>();

            // Convert request headers to JSON using System.Text.Json
            JsonObject headers = JsonNode.Parse(GetJsonString(request.Headers))?.AsObject() ?? new JsonObject();

            string url = request.GetEncodedUrl().Replace(request.Host.ToString(), BASE_HOST);
            List<string> hostSegments = new List<string> { BASE_HOST };
            List<string> pathSegments = request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

            List<PostmanQuerySection> queries = request.Query
                .Select(k => new PostmanQuerySection
                {
                    Key = k.Key,
                    Value = k.Value.ToString()
                })
                .ToList();

            string baseHostWithProtocol = request.Scheme + "://" + PostmanImp.BASE_HOST;

            foreach (var header in headers)
            {
                string key = header.Key.StartsWith(':') ? header.Key.TrimStart(':') : header.Key;
                string value = header.Value?.ToString() ?? string.Empty;

                if (key.Equals("host", StringComparison.OrdinalIgnoreCase))
                {
                    value = PostmanImp.BASE_HOST;
                }
                else if (key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                {
                    value = url;
                }
                else if (key.Equals("X-ORIGINAL-HOST", StringComparison.OrdinalIgnoreCase))
                {
                    value = baseHostWithProtocol;
                }
                else if (key.Equals("Origin", StringComparison.OrdinalIgnoreCase))
                {
                    value = baseHostWithProtocol;
                }
                else if (key.Equals("cookie", StringComparison.OrdinalIgnoreCase))
                {
                    _CookieValue = value;
                    value = PostmanImp.COOKIE;
                }

                pHeaders.Add(new PostmanHeaderSection
                {
                    Key = key,
                    Value = value
                });
            }
            #endregion
        }


        private async Task<string> GetBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            string body = string.Empty;

            // Leave the body open so the next middleware can read it.
            using (var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                // Do some processing with body…
                // Reset the request body stream position so the next middleware can read it
                request.Body.Position = 0;
            }
            request.EnableBuffering();
            return body;
        }

        private string GetJsonString(object model)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Ignore null values
                WriteIndented = true, // Equivalent to Formatting.Indented
                ReferenceHandler = ReferenceHandler.Preserve, // Ignore reference loops
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Equivalent to CamelCaseNamingStrategy
            };

            return JsonSerializer.Serialize(model, options);
        }

    }
}
