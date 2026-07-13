using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record ClientSecurityStatePayload(
    [property: JsonPropertyName("initiatorIsSecureContext")] bool InitiatorIsSecureContext,
    [property: JsonPropertyName("initiatorIPAddressSpace")] string? InitiatorIpAddressSpace,
    [property: JsonPropertyName("privateNetworkRequestPolicy")] string? PrivateNetworkRequestPolicy);
