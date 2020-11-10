using Newtonsoft.Json;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Models.TestScriptModels
{
    public class Event
    {
        [JsonProperty("listen")]
        public string Type { get; set; }

        [JsonProperty("script")]
        public Script ScriptObject { get; set; }
    }
}
