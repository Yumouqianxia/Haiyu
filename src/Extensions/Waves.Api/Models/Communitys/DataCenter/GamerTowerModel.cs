using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Waves.Api.Models.Communitys.DataCenter;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class DifficultyList
{
    [JsonPropertyName("difficulty")]
    public int Difficulty { get; set; }

    [JsonPropertyName("difficultyName")]
    public string DifficultyName { get; set; }

    [JsonPropertyName("towerAreaList")]
    public List<TowerAreaList> TowerAreaList { get; set; }
}

public class TowerRoleList
{
    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; }

    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }
}

public class FloorList
{
    [JsonPropertyName("floor")]
    public int Floor { get; set; }

    [JsonPropertyName("picUrl")]
    public string PicUrl { get; set; }

    [JsonPropertyName("roleList")]
    public List<TowerRoleList> RoleList { get; set; }

    [JsonPropertyName("star")]
    public int Star { get; set; }
}

public class GamerTowerModel
{
    [JsonPropertyName("difficultyList")]
    public List<DifficultyList> DifficultyList { get; set; }

    [JsonPropertyName("isUnlock")]
    public bool IsUnlock { get; set; }

    [JsonPropertyName("seasonEndTime")]
    public long SeasonEndTime { get; set; }
}

public class TowerAreaList
{
    [JsonPropertyName("areaId")]
    public int AreaId { get; set; }

    [JsonPropertyName("areaName")]
    public string AreaName { get; set; }

    [JsonPropertyName("floorList")]
    public List<FloorList> FloorList { get; set; }

    [JsonPropertyName("maxStar")]
    public int MaxStar { get; set; }

    [JsonPropertyName("star")]
    public int Star { get; set; }
}
