using System;
using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions;

public class ResponseMessageBase
{
    [JsonPropertyName("serverrequestid")]
    public string? ServerRequestID { get; set; }

    [JsonPropertyName("clientrequestid")]
    public string? ClientRequestID { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("statuscode")]
    public uint StatusCode { get; set; }

    [JsonPropertyName("shortmessage")]
    public string? ShortMessage { get; set; }

    [JsonPropertyName("longmessage")]
    public string? LongMessage { get; set; }

    [JsonIgnore]
    public bool IsSuccess => Status?.Equals("success", StringComparison.OrdinalIgnoreCase) ?? false;

    [JsonIgnore]
    public bool IsError => !IsSuccess;
}
