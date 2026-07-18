using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record BlockedSetCookieWithReasonPayload(
    [property: JsonPropertyName("blockedReasons")] List<string> BlockedReasons,
    [property: JsonPropertyName("cookieLine")] string CookieLine,
    [property: JsonPropertyName("cookie")] CookiePayload? Cookie = null);
