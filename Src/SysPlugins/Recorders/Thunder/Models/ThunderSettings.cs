using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder.Models
{
    public class ThunderSettings
    {
        [JsonProperty("tests")]
        public List<ThunderTest> ThunderTests { get; set; }

        [JsonProperty("preReq")]
        public ThunderPreReq PreReq { get; set; }

        [JsonProperty("postReq")]
        public ThunderPreReq PostReq { get; set; }
    }
}
