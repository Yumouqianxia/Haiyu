using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record CdpCommandResponse<T>(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("result")] T? Result,
    [property: JsonPropertyName("error")] CdpErrorObject? Error);
