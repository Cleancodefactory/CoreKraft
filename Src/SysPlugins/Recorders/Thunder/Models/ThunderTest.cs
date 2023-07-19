using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder.Models
{
    public class ThunderTest
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("custom")]
        public string Custom { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
