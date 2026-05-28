using System.Text.Json.Serialization;

namespace Waves.Api.Models.Record;

public class RecordRequest
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

    [JsonPropertyName("cardPoolId")]
    public string CardPoolId { get; set; }

    [JsonPropertyName("serverId")]
    public string ServerId { get; set; }

    [JsonPropertyName("languageCode")]
    public string Language { get; set; }

    [JsonPropertyName("recordId")]
    public string RecordId { get; set; }

    [JsonPropertyName("cardPoolType")]
    public int CardPoolType { get; set; }
}
