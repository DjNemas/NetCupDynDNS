using System;
using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions
{
    public class ResponseMessage<T> : ResponseMessageBase
    {
        [JsonPropertyName("responsedata")]
        public T? ResponseData { get; set; }
    }
}
