using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys.DataCenter;

public class TowerIndexRoleList
{
    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; }

    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }
}

public class GamerTowerIndexData
{
    [JsonPropertyName("difficultyList")]
    public List<DifficultyList> DifficultyList { get; set; }

    [JsonPropertyName("isUnlock")]
    public bool IsUnlock { get; set; }

    [JsonPropertyName("seasonEndTime")]
    public int SeasonEndTime { get; set; }
}
