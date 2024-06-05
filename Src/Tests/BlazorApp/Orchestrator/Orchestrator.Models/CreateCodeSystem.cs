using System.Text.Json.Serialization;

namespace Orchestrator.Models
{
    public class CreateCodeSystem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("code_system")]
        public string CodeSystem { get; set; }

        [JsonPropertyName("code_system_version")]
        public string CodeSystemVersion { get; set; }

        [JsonPropertyName("display")]
        public string Display { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("valueset")]
        public string ValueSet { get; set; }

        [JsonPropertyName("valueset_version")]
        public string ValuesetVersion { get; set; }
    }
}
