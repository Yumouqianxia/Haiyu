using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record NetworkEnableParams(
    [property: JsonPropertyName("maxTotalBufferSize")] int? MaxTotalBufferSize = null,
    [property: JsonPropertyName("maxResourceBufferSize")] int? MaxResourceBufferSize = null,
    [property: JsonPropertyName("maxPostDataSize")] int? MaxPostDataSize = null);
