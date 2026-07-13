using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record CorsErrorStatus(
    [property: JsonPropertyName("corsError")] string CorsError,
    [property: JsonPropertyName("failedParameter")] string? FailedParameter);
