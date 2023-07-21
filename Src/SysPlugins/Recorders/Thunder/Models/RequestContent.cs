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
        public RequestContent()
        {
            ContainerId = string.Empty;
        }

        [JsonProperty("_id")]
        public string Id { get { return Guid.NewGuid().ToString(); } }

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
                return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).ToString("o");
            }
        }

        [JsonProperty("modified")]
        public string Modified
        {
            get
            {
                return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).ToString("o");
            }
        }

        [JsonProperty("headers")]
        public List<ThunderHeaderSection> Headers { get; internal set; }

        [JsonProperty("params")]
        public List<ThunderQuerySection> Params { get; internal set; }

        [JsonProperty("body")]
        public ThunderBody Body { get; internal set; }
    }
}
