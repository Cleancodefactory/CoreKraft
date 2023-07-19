using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder.Models
{
    public class ThunderPreReq
    {
        [JsonProperty("runFilters")]
        public List<string> RunFilters { get; set; }

        [JsonProperty("options")]
        public ThunderOption ThunderOptions { get; set; }
    }
}
