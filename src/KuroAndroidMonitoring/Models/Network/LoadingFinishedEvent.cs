using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record LoadingFinishedEvent(
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("timestamp")] double Timestamp,
    [property: JsonPropertyName("encodedDataLength")] double EncodedDataLength);
