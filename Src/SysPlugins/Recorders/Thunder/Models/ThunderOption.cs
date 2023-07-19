using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder.Models
{
    public class ThunderOption
    {
        [JsonProperty("clearCookies")]
        public bool ClearCookies { get; set; }

    }
}
