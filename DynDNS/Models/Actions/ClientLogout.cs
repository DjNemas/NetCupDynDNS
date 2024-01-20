using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions
{
    public class ClientLogout
    {
        [JsonPropertyName("customernumber")]
        public uint CustomerNumber { get; set; }
        [JsonPropertyName("apisessionid")]
        public string ApiSessionID { get; set; }
        [JsonPropertyName("apikey")]
        public string ApiKey { get; set; }
    }
}
