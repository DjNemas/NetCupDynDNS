using System.Text.Json.Serialization;

namespace DynDNS.Models.DNSInfoResponse
{
    public class DNSRecordData
    {
        [JsonPropertyName("id")]
        public required string ID { get; set; }

        [JsonPropertyName("hostname")]
        public required string Hostname { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("priority")]
        public required string Priority { get; set; }

        [JsonPropertyName("destination")]
        public required string Destination { get; set; }

        [JsonPropertyName("deleterecord")]
        public bool DeleteRecord { get; set; }

        [JsonPropertyName("state")]
        public required string State { get; set; }
    }
}
