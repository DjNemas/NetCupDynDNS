using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions
{
    public class ClientLogout
    {
        [JsonPropertyName("customernumber")]
        public uint CustomerNumber { get; set; }
        [JsonPropertyName("apisessionid")]
        public required string ApiSessionID { get; set; }
        [JsonPropertyName("apikey")]
        public required string ApiKey { get; set; }
    }
}
