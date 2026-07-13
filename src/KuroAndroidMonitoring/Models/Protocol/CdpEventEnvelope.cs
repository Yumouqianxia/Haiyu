using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record CdpEventEnvelope(
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] JsonElement Params);
