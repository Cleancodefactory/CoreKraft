using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ccf.Ck.Processing.Web.Request.Primitives
{
    internal class BatchRequest
    {
        [JsonProperty("url")]
        internal string Url { get; set; }

        [JsonProperty("params")]
        internal Dictionary<string, object> Params { get; set; }

        [JsonProperty("data")]
        internal Dictionary<string, object> Data { get; set; }
    }
}
