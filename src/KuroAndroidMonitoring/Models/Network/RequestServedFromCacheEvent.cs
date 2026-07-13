using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record RequestServedFromCacheEvent(
    [property: JsonPropertyName("requestId")] string RequestId);
