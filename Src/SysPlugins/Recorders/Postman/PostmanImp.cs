using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Recorders.Postman.Models;
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
        private static PostmanRunnerModel _RunnerModel = new PostmanRunnerModel();

        public Task<string> GetFinalResult()
        {
            string result = GetJsonString(_RunnerModel);
            WriteToFileAsync(result);
            _RunnerModel = new PostmanRunnerModel();
            return Task.FromResult(result);
        }

        public async Task HandleRequest(HttpRequest request)
        {
            #region Setting up required data from the HttpRequest

            string method = request.Method;

            List<PostmanHeaderSection> pHeaders = new List<PostmanHeaderSection>();
            JObject headers = JObject.Parse(GetJsonString(request.Headers));

            string body = await GetBodyAsync(request);

            string url = request.GetEncodedUrl();
            List<string> hostSegments = request.Host.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList(); 
            List<string> pathSegments = request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList(); 
            Dictionary<string, string> queries = request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());         

            foreach (var header in headers)
            {
                pHeaders.Add(new PostmanHeaderSection
                {
                    Key = header.Key,

                    // If the key is cookie set the value to the dynamic variable set in postman
                    Value = header.Key == "cookie" ? "{{OAuth_Token}}" : header.Value.First.ToString()
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

            // Add newly constructed Request and the url to the Runner Model 
            _RunnerModel.PostmanItemRequests.Add(new PostmanRequest
            {
                Name = url,
                RequestContent = requestContent
            });
        }

        private async void WriteToFileAsync(string content)
        {
            using StreamWriter file =
            new StreamWriter(@"D:\Test.json", true);
            await file.WriteLineAsync(content);
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

            return body;
        }

        private string GetJsonString(object model)
        {
            string jsonString = JsonConvert.SerializeObject(model, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Include,
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
