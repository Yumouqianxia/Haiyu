using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record ExemptedSetCookieWithReasonPayload(
    [property: JsonPropertyName("exemptionReason")] string ExemptionReason,
    [property: JsonPropertyName("cookieLine")] string CookieLine,
    [property: JsonPropertyName("cookie")] CookiePayload? Cookie = null);
