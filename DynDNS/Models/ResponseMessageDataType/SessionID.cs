using System.Text.Json.Serialization;

namespace DynDNS.Models.ResponseMessageDataType
{
    public class SessionID
    {
        [JsonPropertyName("apisessionid")]
        public string ApiSessionID { get; set; }
    }
}
