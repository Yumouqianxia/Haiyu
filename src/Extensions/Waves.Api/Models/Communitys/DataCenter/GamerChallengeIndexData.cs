using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys.DataCenter;

public class ChallengeList
{
    [JsonPropertyName("country")]
    public Country Country { get; set; }

    [JsonPropertyName("indexList")]
    public List<IndexList> IndexList { get; set; }
}

public class IndexList
{
    [JsonPropertyName("bossHeadIcon")]
    public string BossHeadIcon { get; set; }

    [JsonPropertyName("bossIconUrl")]
    public string BossIconUrl { get; set; }

    [JsonPropertyName("bossId")]
    public int BossId { get; set; }

    [JsonPropertyName("bossLevel")]
    public int BossLevel { get; set; }

    [JsonPropertyName("bossName")]
    public string BossName { get; set; }

    [JsonPropertyName("contryId")]
    public int ContryId { get; set; }

    [JsonPropertyName("difficulty")]
    public int Difficulty { get; set; }
}

public class GamerChallengeIndexData
{
    [JsonPropertyName("challengeList")]
    public List<ChallengeList> ChallengeList { get; set; }

    [JsonPropertyName("isUnlock")]
    public bool IsUnlock { get; set; }

    [JsonPropertyName("open")]
    public bool Open { get; set; }

    [JsonPropertyName("wikiUrl")]
    public string WikiUrl { get; set; }
}
