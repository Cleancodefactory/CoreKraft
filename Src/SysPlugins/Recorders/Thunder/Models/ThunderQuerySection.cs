using Newtonsoft.Json;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder.Models
{
    public class ThunderQuerySection
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
