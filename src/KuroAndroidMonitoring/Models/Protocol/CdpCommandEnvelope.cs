using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record CdpCommandEnvelope<TParams>(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] TParams? Params);
