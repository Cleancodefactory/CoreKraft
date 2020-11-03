using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Models
{
    public class PostmanUrlSection
    {
        public string Raw { get; set; }
        public string Protocol { get; set; }

        [JsonProperty("host")]
        public List<string> HostSegments { get; set; }

        [JsonProperty("path")]
        public List<string> PathSegments { get; set; }

        [JsonProperty("query")]
        public List<Dictionary<string, string>> Queries = new List<Dictionary<string, string>>();
    }
}
