using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions
{
    public class RequestAction<T>
    {
        [JsonPropertyName("action")]
        public required string Action { get; set; }
        
        [JsonPropertyName("param")]
        public required T Param { get; set; }
    }
}
