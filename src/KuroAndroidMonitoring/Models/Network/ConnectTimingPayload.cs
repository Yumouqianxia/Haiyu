using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record ConnectTimingPayload(
    [property: JsonPropertyName("requestTime")] double RequestTime);
