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
        private string _CollectionId;
        public ThunderRunnerModel()
        {
            Client = "Thunder Client";
            CollectionName = "Thunderclient";
            Version = "1.1";
            Folders = new List<string>();
            ThunderRequests = new List<RequestContent>(100);
            _CollectionId = Guid.NewGuid().ToString();
        }

        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("collectionName")]
        public string CollectionName { get; set; }

        [JsonProperty("dateExported")]
        public string DateExported
        {
            get
            {
                return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).ToString("o");
            }
        }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("folders")]
        public List<string> Folders { get; set; }

        [JsonProperty("requests")]
        public List<RequestContent> ThunderRequests { get; set; }

        [JsonProperty("settings")]
        public ThunderSettings ThunderSettings { get; set; }

        internal int SortNum { get; set; }

        internal string CollectionId
        {
            get
            {
                return _CollectionId;
            }
        }

        [JsonProperty("cookie")]
        public string Cookie { get; set; }
    }
}
