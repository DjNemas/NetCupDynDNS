using System.Text.Json.Serialization;

namespace DynDNS.Models.DNSInfoResponse
{
    public class DNSRecords
    {
        [JsonPropertyName("dnsrecords")]
        public DNSRecordData[] DnsRecords { get; set; }
    }
}
