using System.Text.Json.Serialization;

namespace Waves.Api.Models.CloudGame;

public class ApplyQualityProfilePayload
{
    [JsonPropertyName("bitRate")]
    public int BitRate { get; set; }

    [JsonPropertyName("bitRateMin")]
    public int BitRateMin { get; set; }

    [JsonPropertyName("bitRateMax")]
    public int BitRateMax { get; set; }

    [JsonPropertyName("fps")]
    public int Fps { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("codecType")]
    public int CodecType { get; set; }

    [JsonPropertyName("streamStrategy")]
    public string StreamStrategy { get; set; } = string.Empty;

    [JsonPropertyName("enableImageEnhancement")]
    public bool EnableImageEnhancement { get; set; }

    [JsonPropertyName("dpi")]
    public int Dpi { get; set; }
}
