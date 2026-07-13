using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record RequestPayload(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("headers")] Dictionary<string, object?> Headers,
    [property: JsonPropertyName("postData")] string? PostData = null,
    [property: JsonPropertyName("hasPostData")] bool? HasPostData = null,
    [property: JsonPropertyName("mixedContentType")] string? MixedContentType = null,
    [property: JsonPropertyName("initialPriority")] string? InitialPriority = null,
    [property: JsonPropertyName("referrerPolicy")] string? ReferrerPolicy = null);
