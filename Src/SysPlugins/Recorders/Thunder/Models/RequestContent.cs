using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder.Models
{
    public class RequestContent
    {
        [JsonProperty("_id")]
        public string Id { get { return new Guid().ToString(); } }

        [JsonProperty("colId")]
        public string ColId { get; set; }

        [JsonProperty("containerId")]
        public string ContainerId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("sortNum")]
        public int SortNum { get; set; }

        [JsonProperty("created")]
        public string Created
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            }
        }

        [JsonProperty("modified")]
        public string Modified
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            }
        }
    }
}
