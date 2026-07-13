using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record ResponseReceivedExtraInfoEvent(
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("blockedCookies")] List<BlockedSetCookieWithReasonPayload>? BlockedCookies,
    [property: JsonPropertyName("headers")] Dictionary<string, string> Headers,
    [property: JsonPropertyName("resourceIPAddressSpace")] string? ResourceIpAddressSpace,
    [property: JsonPropertyName("statusCode")] int StatusCode,
    [property: JsonPropertyName("headersText")] string? HeadersText = null,
    [property: JsonPropertyName("cookiePartitionKey")] string? CookiePartitionKey = null,
    [property: JsonPropertyName("cookiePartitionKeyOpaque")] bool? CookiePartitionKeyOpaque = null,
    [property: JsonPropertyName("exemptedCookies")] List<ExemptedSetCookieWithReasonPayload>? ExemptedCookies = null);
