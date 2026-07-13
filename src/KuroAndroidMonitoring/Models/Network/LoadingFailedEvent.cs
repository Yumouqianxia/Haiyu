using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record LoadingFailedEvent(
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("timestamp")] double Timestamp,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("errorText")] string ErrorText,
    [property: JsonPropertyName("canceled")] bool? Canceled = null,
    [property: JsonPropertyName("blockedReason")] string? BlockedReason = null,
    [property: JsonPropertyName("corsErrorStatus")] CorsErrorStatus? CorsErrorStatus = null);
