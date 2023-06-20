using Ccf.Ck.SysPlugins.Recorders.Postman.Models.TestScriptModels;
using Ccf.Ck.SysPlugins.Recorders.Postman.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

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

        [JsonProperty("item")]
        public List<PostmanRequest> PostmanItemRequests { get; set; }

        [JsonProperty("auth")]
        public PostmanAuthenticationSection AuthenticationSection { get; private set; }

        [JsonProperty("event")]
        List<Event> PreRequestEvent { get; set; }

        private void SetPreRequestEvents(string preRequestEvent)
        {
            List<Event> events = JsonConvert.DeserializeObject<List<Event>>(preRequestEvent);
            this.PreRequestEvent = events;
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
