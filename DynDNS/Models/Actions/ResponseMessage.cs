using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DynDNS.Models.Actions
{
    public class ResponseMessage<T>
    {
        [JsonPropertyName("serverrequestid")]
        private string ServerRequestID { get; set; }

        [JsonPropertyName("clientrequestid")]
        public string ClientRequestID { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("statuscode")]
        public uint StatusCode { get; set; }

        [JsonPropertyName("shortmessage")]
        public string ShortMessage { get; set; }

        [JsonPropertyName("longmessage")]
        public string LongMessage { get; set; }

        [JsonPropertyName("responsedata")]
        public T ResponseData { get; set; }
    }
}
