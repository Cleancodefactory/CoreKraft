using Ccf.Ck.SysPlugins.Recorders.Postman.Models.TestScriptModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Models
{
    public class PostmanRunnerModel
    {
        private readonly string preRequestEvent = File.ReadAllText("..\\..\\SysPlugins\\Recorders\\Postman\\Models\\SeedEventsJsons\\PreRequest.json");

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
            var events = JsonConvert.DeserializeObject<List<Event>>(preRequestEvent);
            this.PreRequestEvent = events;
        }
    }
}
