using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record NetworkResponsePayload(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("statusText")] string? StatusText,
    [property: JsonPropertyName("mimeType")] string? MimeType,
    [property: JsonPropertyName("headers")] Dictionary<string, object?>? Headers = null,
    [property: JsonPropertyName("protocol")] string? Protocol = null,
    [property: JsonPropertyName("remoteIPAddress")] string? RemoteIpAddress = null,
    [property: JsonPropertyName("remotePort")] int? RemotePort = null,
    [property: JsonPropertyName("fromDiskCache")] bool? FromDiskCache = null,
    [property: JsonPropertyName("fromServiceWorker")] bool? FromServiceWorker = null,
    [property: JsonPropertyName("fromPrefetchCache")] bool? FromPrefetchCache = null,
    [property: JsonPropertyName("encodedDataLength")] double? EncodedDataLength = null);
