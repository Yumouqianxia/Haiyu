using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record AssociatedCookiePayload(
    [property: JsonPropertyName("cookie")] CookiePayload? Cookie,
    [property: JsonPropertyName("blockedReasons")] List<string>? BlockedReasons,
    [property: JsonPropertyName("exemptionReason")] string? ExemptionReason = null);
