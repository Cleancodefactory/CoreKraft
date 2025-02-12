using Ccf.Ck.SysPlugins.Recorders.Postman.Models.TestScriptModels;
using Ccf.Ck.SysPlugins.Recorders.Postman.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Models
{
    public class PostmanRunnerModel
    {
        private readonly string preRequestEvent = ResourceReader.GetResource("PreRequest");

        public PostmanRunnerModel()
        {
            this.Info = new Dictionary<string, string>()
            {
                { "_postman_id", Guid.NewGuid().ToString() },
                { "name", "PostmanAutomationTests" },
                { "schema", @"https://schema.getpostman.com/json/collection/v2.1.0/collection.json" }
            };

            this.PostmanItemRequests = new List<PostmanRequest>();

            this.AuthenticationSection = new PostmanAuthenticationSection()
            {
                Type = "oauth2",
                TypeDefinitions = new List<PostmanTypeDefinition>()
                {
                    new PostmanTypeDefinition
                    {
                        Key = "addTokenTo",
                        Value = "header",
                        Type = "string"
                    }
                }
            };

            SetPreRequestEvents(this.preRequestEvent);
        }

        public Dictionary<string, string> Info { get; private set; }

        [JsonPropertyName("item")]
        public List<PostmanRequest> PostmanItemRequests { get; set; }

        [JsonPropertyName("auth")]
        public PostmanAuthenticationSection AuthenticationSection { get; private set; }

        [JsonPropertyName("event")]
        List<Event> PreRequestEvent { get; set; }


        private void SetPreRequestEvents(string preRequestEvent)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Ensures case-insensitive property mapping
            };

            List<Event>? events = System.Text.Json.JsonSerializer.Deserialize<List<Event>>(preRequestEvent, options);
            this.PreRequestEvent = events ?? new List<Event>(); // Ensures it is not null
        }


        public void UpdatePreRequestEvents(string cookie)
        {
            for (int i = 0; i < this.PreRequestEvent.Count; i++)
            {
                var preEvent = this.PreRequestEvent[i];
                for (int j = 0; j < preEvent.ScriptObject.Executions.Count; j++)
                {
                    preEvent.ScriptObject.Executions[j] = preEvent.ScriptObject.Executions[j].Replace(PostmanImp.COOKIE, "'" + cookie + "'");
                }
            }
        }
    }
}
