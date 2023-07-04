using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Recorders.Postman.Models;
using Ccf.Ck.SysPlugins.Recorders.Postman.Models.TestScriptModels;
using Ccf.Ck.SysPlugins.Recorders.Postman.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            JObject headers = JObject.Parse(GetJsonString(request.Headers));

            string url = request.GetEncodedUrl().Replace(request.Host.ToString(), BASE_HOST);
            List<string> hostSegments = new List<string>{BASE_HOST};//request.Host.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> pathSegments = request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
            List<PostmanQuerySection> queries = request.Query.Select(k => new PostmanQuerySection
            {
                Key = k.Key,
                Value = k.Value.ToString()
            }).ToList();

            foreach (var header in headers)
            {
                string key = header.Key.StartsWith(':') ? header.Key.TrimStart(':') : header.Key;
                string value = header.Value.First.ToString();
                if (key != null && key.Equals("host", StringComparison.OrdinalIgnoreCase))
                {
                    value = PostmanImp.BASE_HOST;
                }
                else if (key != null && key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                {
                    value = url;
                }
                else if (key != null && key.Equals("X-ORIGINAL-HOST", StringComparison.OrdinalIgnoreCase))
                {
                    value = PostmanImp.BASE_HOST;
                }
                else if (key != null && key.Equals("cookie", StringComparison.OrdinalIgnoreCase))
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

            PostmanBuilder builder = new PostmanBuilder();

            // Construct the Request entity
            RequestContent requestContent = builder
                .MethodBuilder
                    .AddMethod(method)
                .HeaderBuilder
                    .AddHeader(pHeaders)
                .BodyBuilder
                    .AddBody("raw", body)
                .UrlBuilder
                    .AddUrlData(url, request.Scheme, pathSegments, hostSegments, queries);

            // Add the first event for script to the first request
            if (_RunnerModel.PostmanItemRequests.Count < 1)
            {
                var firstRequestEvent = JsonConvert.DeserializeObject<List<Event>>(ResourceReader.GetResource("FirstRequest"));

                _RunnerModel.PostmanItemRequests.Add(new PostmanRequest
                {
                    Name = url,
                    FirstRequestEvent = firstRequestEvent,
                    RequestContent = requestContent
                });

                return;
            }

            // Add newly constructed Request and the url to the Runner Model 
            _RunnerModel.PostmanItemRequests.Add(new PostmanRequest
            {
                Name = url,
                RequestContent = requestContent
            });
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
            string jsonString = JsonConvert.SerializeObject(model, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });

            return jsonString;
        }
    }
}
