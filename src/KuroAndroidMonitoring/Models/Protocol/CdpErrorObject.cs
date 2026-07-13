using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record CdpErrorObject(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("data")] JsonElement Data);
