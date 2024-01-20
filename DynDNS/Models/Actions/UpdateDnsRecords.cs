using DynDNS.Models.DNSInfoResponse;
using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions
{
    public class UpdateDnsRecords
    {
        [JsonPropertyName("domainname")]
        public string DomainName { get; set; }

        [JsonPropertyName("customernumber")]
        public uint CustonmerNumber { get; set; }

        [JsonPropertyName("apikey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("apisessionid")]
        public string ApiSessionID { get; set; }

        [JsonPropertyName("dnsrecordset")]
        public DNSRecords DNSRecordSet { get; set; }
    }
}
