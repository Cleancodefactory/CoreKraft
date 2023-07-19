using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Recorders.Thunder.Models
{
    public class ThunderRunnerModel
    {
        public ThunderRunnerModel()
        {
            Client = "Thunder Client";
            CollectionName = "Thunderclient";
            DateExported = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            Version = "1.1";
            Folders = new List<string>();
            ThunderRequests = new List<ThunderRequest>(100);
        }

        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("collectionName")]
        public string CollectionName { get; set; }

        [JsonProperty("dateExported")]
        public string DateExported { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("folders")]
        public List<string> Folders { get; set; }

        [JsonProperty("requests")]
        public List<ThunderRequest> ThunderRequests { get; set; }

        [JsonProperty("settings")]
        public ThunderSettings ThunderSettings { get; set; }


        internal void UpdateSettings(string cookieValue)
        {
            throw new NotImplementedException();
        }
    }
}
