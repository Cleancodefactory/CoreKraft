using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Recorders.Thunder.Models;
using Ccf.Ck.Utilities.Json;
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
using System.Text.Json;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder
{
    public class ThunderImp : IRequestRecorder
    {
        public const string BASE_HOST = "{{base_host}}";
        public const string COOKIE = "{{cookie}}";
        private string _CookieValue;
        private ThunderRunnerModel _RunnerModel = new ThunderRunnerModel();

        public bool IsRunning { get; set; }

        public Task<string> GetFinalResult()
        {
            if (_CookieValue != null)
            {
                _RunnerModel.Cookie = _CookieValue;
            }
            _RunnerModel.ThunderSettings = new ThunderSettings();
            _RunnerModel.ThunderSettings.ThunderTests = CreateTests();
            _RunnerModel.ThunderSettings.PreReq = CreatePreReq();
            _RunnerModel.ThunderSettings.PostReq = CreatePostReq();
            string result = GetJsonString(_RunnerModel);
            _RunnerModel = new ThunderRunnerModel();
            return Task.FromResult(result);
        }

        public async Task HandleRequest(HttpRequest request)
        {
            #region Setting up required data from the HttpRequest
            string body = await GetBodyAsync(request);

            List<ThunderHeaderSection> pHeaders = new List<ThunderHeaderSection>();
            JObject headers = JObject.Parse(GetJsonString(request.Headers));

            string url = request.GetEncodedUrl().Replace(request.Host.ToString(), BASE_HOST);
            List<string> hostSegments = new List<string> { BASE_HOST };//request.Host.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> pathSegments = request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
            List<ThunderQuerySection> queries = request.Query.Select(k => new ThunderQuerySection
            {
                Name = k.Key,
                Value = k.Value.ToString()
            }).ToList();

            string baseHostWithProtocol = request.Scheme + "://" + ThunderImp.BASE_HOST;
            foreach (var header in headers)
            {
                string key = header.Key.StartsWith(':') ? header.Key.TrimStart(':') : header.Key;
                string value = header.Value.First.ToString();
                if (key != null && key.Equals("host", StringComparison.OrdinalIgnoreCase))
                {
                    value = ThunderImp.BASE_HOST;
                }
                else if (key != null && key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                {
                    value = url;
                }
                else if (key != null && key.Equals("X-ORIGINAL-HOST", StringComparison.OrdinalIgnoreCase))
                {
                    value = baseHostWithProtocol;
                }
                else if (key != null && key.Equals("Origin", StringComparison.OrdinalIgnoreCase))
                {
                    value = baseHostWithProtocol;
                }
                else if (key != null && key.Equals("cookie", StringComparison.OrdinalIgnoreCase))
                {
                    _CookieValue = value;
                    value = ThunderImp.COOKIE;
                }
                pHeaders.Add(new ThunderHeaderSection
                {
                    Name = key,
                    Value = value
                });
            }
            #endregion

            // Construct the Request entity
            RequestContent requestContent = new RequestContent();
            requestContent.ColId = _RunnerModel.CollectionId;
            requestContent.Name = url;
            requestContent.Url = url;
            requestContent.Method = request.Method;
            _RunnerModel.SortNum += 10000;
            requestContent.SortNum = _RunnerModel.SortNum;
            requestContent.Headers = pHeaders;
            requestContent.Params = queries;
            requestContent.Body = new ThunderBody { Raw = body };

            _RunnerModel.ThunderRequests.Add(requestContent);
        }

        #region Private
        private ThunderPreReq CreatePostReq()
        {
            ThunderPreReq thunderPostReq = new ThunderPreReq();
            thunderPostReq.RunFilters.Add(""); //postRequest
            return thunderPostReq;
        }

        private ThunderPreReq CreatePreReq()
        {
            ThunderPreReq thunderPreReq = new ThunderPreReq();
            thunderPreReq.RunFilters.Add(""); //preRequest
            thunderPreReq.ThunderOptions = new ThunderOption { ClearCookies = true };
            return thunderPreReq;
        }

        private List<ThunderTest> CreateTests()
        {
            List<ThunderTest> tests = new List<ThunderTest>();
            ThunderTest test1 = new ThunderTest();
            test1.Type = "json-query";
            test1.Custom = "json.packet.status._issuccessful";
            test1.Action = "equal";
            test1.Value = "1";
            tests.Add(test1);

            ThunderTest test2 = new ThunderTest();
            test2.Type = "res-code";
            test2.Custom = "";
            test2.Action = "equal";
            test2.Value = "200";
            tests.Add(test2);

            ThunderTest test3 = new ThunderTest();
            test3.Type = "res-time";
            test3.Custom = "";
            test3.Action = "<";
            test3.Value = "1000";
            tests.Add(test3);

            return tests;
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
                string temp = await reader.ReadToEndAsync();
                if (!string.IsNullOrEmpty(temp))
                {
                    var options = new JsonReaderOptions
                    {
                        AllowTrailingCommas = true,
                        CommentHandling = JsonCommentHandling.Skip
                    };
                    var result = DictionaryStringObjectJson.Deserialize(temp, options);
                    body = System.Text.Json.JsonSerializer.Serialize(result);
                }                

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

        #endregion Private
    }
}
