using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record DataReceivedEvent(
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("timestamp")] double Timestamp,
    [property: JsonPropertyName("dataLength")] int DataLength,
    [property: JsonPropertyName("encodedDataLength")] int EncodedDataLength);
