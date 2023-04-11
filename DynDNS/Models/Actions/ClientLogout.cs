using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
