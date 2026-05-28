using System.Text.Json.Serialization;
using Waves.Api.JsonConverter;

namespace Waves.Api.Models.Communitys.DataCenter;

public class Detilys
{
    [JsonPropertyName("bossHeadIcon")]
    public string BossHeadIcon { get; set; }

    [JsonPropertyName("bossIconUrl")]
    public string BossIconUrl { get; set; }

    [JsonPropertyName("bossLevel")]
    public int BossLevel { get; set; }

    [JsonPropertyName("bossName")]
    public string BossName { get; set; }

    [JsonPropertyName("challengeId")]
    public int ChallengeId { get; set; }

    [JsonPropertyName("difficulty")]
    public int Difficulty { get; set; }

    [JsonPropertyName("passTime")]
    public int PassTime { get; set; }

    [JsonPropertyName("roles")]
    public List<ChallengeRole> Roles { get; set; }

    [JsonIgnore]
    public string BossId { get; set; }
}

[JsonConverter(typeof(GamerChallengeDetilyConverter))]
public class ChallengeInfo
{
    public List<Detilys> Detilys { get; set; }
}

public class ChallengeRole
{
    [JsonPropertyName("natureId")]
    public int NatureId { get; set; }

    [JsonPropertyName("roleHeadIcon")]
    public string RoleHeadIcon { get; set; }

    [JsonPropertyName("roleLevel")]
    public int RoleLevel { get; set; }

    [JsonPropertyName("roleName")]
    public string RoleName { get; set; }
}

public class GamerChallengeDetily
{
    [JsonPropertyName("challengeInfo")]
    public ChallengeInfo ChallengeInfo { get; set; }

    [JsonPropertyName("isUnlock")]
    public bool IsUnlock { get; set; }

    [JsonPropertyName("open")]
    public bool Open { get; set; }
}
