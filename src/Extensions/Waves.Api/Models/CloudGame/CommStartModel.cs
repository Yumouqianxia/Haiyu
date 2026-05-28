using System.Text.Json.Serialization;

namespace Waves.Api.Models.CloudGame;


public class ResourceData
{
    [JsonPropertyName("wlResourceData")]
    public WlResourceData WlResourceData { get; set; }
}

public class CommStartModel
{
    [JsonPropertyName("nodeList")]
    public List<NodeList> NodeList { get; set; }

    [JsonPropertyName("resourceData")]
    public ResourceData ResourceData { get; set; }

    [JsonPropertyName("payType")]
    public int PayType { get; set; }
}

public class WlResourceData
{
    [JsonPropertyName("tenantKey")]
    public string TenantKey { get; set; }

    [JsonPropertyName("gameId")]
    public string GameId { get; set; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; }

    [JsonPropertyName("bitRate")]
    public int BitRate { get; set; }

    [JsonPropertyName("fps")]
    public int Fps { get; set; }

    [JsonPropertyName("codecType")]
    public int CodecType { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("cmdLine")]
    public string CmdLine { get; set; }

    [JsonPropertyName("bizData")]
    public string BizData { get; set; }
}
