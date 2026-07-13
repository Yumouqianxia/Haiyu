using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record RequestWillBeSentExtraInfoEvent(
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("associatedCookies")] List<AssociatedCookiePayload>? AssociatedCookies,
    [property: JsonPropertyName("headers")] Dictionary<string, string> Headers,
    [property: JsonPropertyName("connectTiming")] ConnectTimingPayload? ConnectTiming = null,
    [property: JsonPropertyName("clientSecurityState")] ClientSecurityStatePayload? ClientSecurityState = null,
    [property: JsonPropertyName("siteHasCookieInOtherPartition")] bool? SiteHasCookieInOtherPartition = null);
