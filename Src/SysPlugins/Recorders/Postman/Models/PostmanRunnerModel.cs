using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Models
{
    public class PostmanRunnerModel
    {
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
        }

        public Dictionary<string, string> Info { get; private set; }

        [JsonProperty("item")]
        public List<PostmanRequest> PostmanItemRequests { get; set; }

        [JsonProperty("auth")]
        public PostmanAuthenticationSection AuthenticationSection { get; private set; }
    }
}
