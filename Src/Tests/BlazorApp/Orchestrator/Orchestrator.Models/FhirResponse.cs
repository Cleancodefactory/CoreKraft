using System.Text.Json.Serialization;

namespace Orchestrator.Models
{
    public class FhirResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
