using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record RequestWillBeSentEvent(
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("loaderId")] string? LoaderId,
    [property: JsonPropertyName("documentURL")] string? DocumentUrl,
    [property: JsonPropertyName("request")] RequestPayload Request,
    [property: JsonPropertyName("timestamp")] double Timestamp,
    [property: JsonPropertyName("wallTime")] double? WallTime,
    [property: JsonPropertyName("initiator")] InitiatorPayload? Initiator,
    [property: JsonPropertyName("redirectHasExtraInfo")] bool? RedirectHasExtraInfo,
    [property: JsonPropertyName("redirectResponse")] NetworkResponsePayload? RedirectResponse,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("frameId")] string? FrameId = null,
    [property: JsonPropertyName("hasUserGesture")] bool? HasUserGesture = null);
