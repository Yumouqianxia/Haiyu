using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record ResponseReceivedEvent(
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("loaderId")] string? LoaderId,
    [property: JsonPropertyName("timestamp")] double Timestamp,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("response")] NetworkResponsePayload Response,
    [property: JsonPropertyName("frameId")] string? FrameId = null);
