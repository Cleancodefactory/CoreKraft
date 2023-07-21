using Newtonsoft.Json;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder.Models
{
    public class ThunderBody
    {
        [JsonProperty("type")]
        public string Type
        {
            get
            {
                return "json";
            }
        }

        [JsonProperty("raw")]
        public string Raw { get; set; }
    }
}
