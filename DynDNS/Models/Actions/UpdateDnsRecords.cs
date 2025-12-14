using DynDNS.Models.DNSInfoResponse;
using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions
{
    public class UpdateDnsRecords
    {
        [JsonPropertyName("domainname")]
        public required string DomainName { get; set; }

        [JsonPropertyName("customernumber")]
        public uint CustonmerNumber { get; set; }

        [JsonPropertyName("apikey")]
        public required string ApiKey { get; set; }

        [JsonPropertyName("apisessionid")]
        public required string ApiSessionID { get; set; }

        [JsonPropertyName("dnsrecordset")]
        public required DNSRecords DNSRecordSet { get; set; }
    }
}
