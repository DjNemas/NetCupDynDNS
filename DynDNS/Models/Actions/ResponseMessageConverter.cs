using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynDNS.Models.Actions;

public class ResponseMessageConverter<T> : JsonConverter<ResponseMessage<T>>
{
    public override ResponseMessage<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var response = new ResponseMessage<T>
        {
            ServerRequestID = root.GetProperty("serverrequestid").GetString(),
            ClientRequestID = root.TryGetProperty("clientrequestid", out var clientReqId) ? clientReqId.GetString() : null,
            Action = root.GetProperty("action").GetString(),
            Status = root.GetProperty("status").GetString(),
            StatusCode = root.GetProperty("statuscode").GetUInt32(),
            ShortMessage = root.GetProperty("shortmessage").GetString(),
            LongMessage = root.TryGetProperty("longmessage", out var longMsg) ? longMsg.GetString() : null
        };

        if (root.TryGetProperty("responsedata", out var responseData))
        {
            if (responseData.ValueKind == JsonValueKind.String && string.IsNullOrEmpty(responseData.GetString()))
            {
                response.ResponseData = default;
            }
            else if (responseData.ValueKind != JsonValueKind.Null)
            {
                var rawText = responseData.GetRawText();
                response.ResponseData = JsonSerializer.Deserialize<T>(rawText, options);
            }
        }

        return response;
    }

    public override void Write(Utf8JsonWriter writer, ResponseMessage<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("serverrequestid", value.ServerRequestID);
        writer.WriteString("clientrequestid", value.ClientRequestID);
        writer.WriteString("action", value.Action);
        writer.WriteString("status", value.Status);
        writer.WriteNumber("statuscode", value.StatusCode);
        writer.WriteString("shortmessage", value.ShortMessage);
        writer.WriteString("longmessage", value.LongMessage);

        writer.WritePropertyName("responsedata");
        if (value.ResponseData is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value.ResponseData, options);
        }

        writer.WriteEndObject();
    }
}
