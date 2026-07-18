using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record GetResponseBodyResult(
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("base64Encoded")] bool Base64Encoded);
