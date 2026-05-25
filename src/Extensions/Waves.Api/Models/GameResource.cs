using System.Text.Json.Serialization;

namespace Waves.Api.Models;

public class Resource
{
    [JsonPropertyName("dest")]
    public string Dest { get; set; }

    [JsonPropertyName("md5")]
    public string Md5 { get; set; }

    [JsonPropertyName("sampleHash")]
    public string SampleHash { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }
}

public class GameResource
{
    [JsonPropertyName("resource")]
    public List<Resource> Resource { get; set; }

    [JsonPropertyName("sampleHashInfo")]
    public SampleHashInfo SampleHashInfo { get; set; }
}

[JsonSerializable(typeof(GameResource))]
[JsonSerializable(typeof(SampleHashInfo))]
[JsonSerializable(typeof(List<Resource>))]
public partial class GameResourceContext : JsonSerializerContext { }

public class SampleHashInfo
{
    [JsonPropertyName("sampleNum")]
    public int SampleNum { get; set; }

    [JsonPropertyName("sampleBlockMaxSize")]
    public int SampleBlockMaxSize { get; set; }
}
