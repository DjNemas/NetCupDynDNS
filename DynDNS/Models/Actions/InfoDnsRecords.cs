using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions
{
    public class InfoDnsRecords
    {
        [JsonPropertyName("domainname")]
        public string DomainName { get; set; }

        [JsonPropertyName("customernumber")]
        public uint CustonmerNumber { get; set; }

        [JsonPropertyName("apikey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("apisessionid")]
        public string ApiSessionID { get; set; }
    }
}
