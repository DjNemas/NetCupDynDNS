using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions
{
    public class ClientLogin
    {
        [JsonPropertyName("customernumber")]
        public uint CustomerNumber { get; set; }
        [JsonPropertyName("apipassword")]
        public required string ApiPassword { get; set; }
        [JsonPropertyName("apikey")]
        public required string ApiKey { get; set; }
    }
}
