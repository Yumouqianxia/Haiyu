using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys;

public class SignInResult
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("msg")]
    public string MSG { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
