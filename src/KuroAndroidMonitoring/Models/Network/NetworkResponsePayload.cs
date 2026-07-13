using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record NetworkResponsePayload(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("statusText")] string? StatusText,
    [property: JsonPropertyName("mimeType")] string? MimeType);
