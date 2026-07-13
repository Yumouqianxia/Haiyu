using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record InitiatorPayload(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("lineNumber")] double? LineNumber = null,
    [property: JsonPropertyName("columnNumber")] double? ColumnNumber = null,
    [property: JsonPropertyName("requestId")] string? RequestId = null);
