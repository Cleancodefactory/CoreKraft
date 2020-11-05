using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Models
{
    public class RequestContent
    {
        public string Method { get; set; }

        [JsonProperty("header")]
        public List<PostmanHeaderSection> Headers { get; set; }

        [JsonProperty("body")]
        public PostmanBodySection PostmanBody { get; set; }

        [JsonProperty("url")]
        public PostmanUrlSection PostmanUrl { get; set; }
    }
}
