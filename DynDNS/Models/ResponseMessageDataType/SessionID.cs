using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DynDNS.Models.ResponseMessageDataType
{
    public class SessionID
    {
        [JsonPropertyName("apisessionid")]
        public string ApiSessionID { get; set; }
    }
}
