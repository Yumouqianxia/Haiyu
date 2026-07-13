using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record CookiePayload(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("expires")] double Expires,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("httpOnly")] bool HttpOnly,
    [property: JsonPropertyName("secure")] bool Secure,
    [property: JsonPropertyName("session")] bool Session,
    [property: JsonPropertyName("sameSite")] string? SameSite = null,
    [property: JsonPropertyName("priority")] string? Priority = null,
    [property: JsonPropertyName("sameParty")] bool? SameParty = null,
    [property: JsonPropertyName("sourceScheme")] string? SourceScheme = null,
    [property: JsonPropertyName("sourcePort")] int? SourcePort = null,
    [property: JsonPropertyName("partitionKey")] string? PartitionKey = null);
