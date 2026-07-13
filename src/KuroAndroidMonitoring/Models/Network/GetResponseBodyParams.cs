using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record GetResponseBodyParams(
    [property: JsonPropertyName("requestId")] string RequestId);
