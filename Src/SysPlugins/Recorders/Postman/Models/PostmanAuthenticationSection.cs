using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Models
{
    public class PostmanAuthenticationSection
    {
        public string Type { get; set; }

        [JsonProperty("oauth2")]
        public List<PostmanTypeDefinition> TypeDefinitions { get; set; }
        [JsonProperty("protocolProfileBehavior")]
        public object ProtocolProfileBehavior { get; set; } = new { };
    }
}
