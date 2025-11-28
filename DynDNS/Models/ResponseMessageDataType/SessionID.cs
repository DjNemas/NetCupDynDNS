using System.Text.Json.Serialization;

namespace DynDNS.Models.ResponseMessageDataType
{
    public class SessionID
    {
        [JsonPropertyName("apisessionid")]
        public required string ApiSessionID { get; set; }
    }
}
