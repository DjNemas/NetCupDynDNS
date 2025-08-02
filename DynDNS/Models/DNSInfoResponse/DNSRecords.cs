using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DynDNS.Models.DNSInfoResponse;

public class DNSRecords
{
    [JsonPropertyName("dnsrecords")]
    public DNSRecordData[] DnsRecords { get; set; }

    public void RemoveHosts(List<string> hostnames)
    {
        DnsRecords = DnsRecords.Where(x => !hostnames.Contains(x.Hostname)).ToArray();
    }
}