using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record CdpIncomingMessage(
    [property: JsonPropertyName("id")] long? Id,
    [property: JsonPropertyName("method")] string? Method,
    [property: JsonPropertyName("params")] JsonElement Params,
    [property: JsonPropertyName("result")] JsonElement Result,
    [property: JsonPropertyName("error")] CdpErrorObject? Error);
