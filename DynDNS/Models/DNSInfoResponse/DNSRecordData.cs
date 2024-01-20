using System.Text.Json.Serialization;

namespace DynDNS.Models.DNSInfoResponse
{
    public class DNSRecordData
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("priority")]
        public string Priority { get; set; }

        [JsonPropertyName("destination")]
        public string Destination { get; set; }

        [JsonPropertyName("deleterecord")]
        public bool DeleteRecord { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
