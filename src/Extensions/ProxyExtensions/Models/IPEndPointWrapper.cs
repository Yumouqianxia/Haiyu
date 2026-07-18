using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProxyExtensions.Models;

public class IPEndPointWrapper
{
    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("ips")]
    public List<string> Ips { get; set; }
}

[JsonSerializable(typeof(IPEndPointWrapper))]
[JsonSerializable(typeof(List<IPEndPointWrapper>))]
public partial class IPEndPointWrapperContext : JsonSerializerContext
{

}
