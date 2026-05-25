using System.Text.Json.Serialization;

namespace Waves.Api.Models.Record;

public class FiveGroupData
{
    [JsonPropertyName("five_group_config")]
    public FiveGroupConfig FiveGroupConfig { get; set; }

    [JsonPropertyName("pool_list")]
    public List<PoolList> PoolList { get; set; }

    [JsonPropertyName("version_pools")]
    public List<VersionPool> VersionPools { get; set; }
}

public class FiveGroupConfig
{
    [JsonPropertyName("five_maps")]
    public List<FiveMap> FiveMaps { get; set; }
}

public class FiveMap
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("item_id")]
    public int ItemId { get; set; }

    [JsonPropertyName("weapon_id")]
    public int WeaponId { get; set; }

    [JsonPropertyName("pool_type")]
    public int? PoolType { get; set; }
}

public class PoolList
{
    [JsonPropertyName("start_at")]
    public string StartAt { get; set; }

    [JsonPropertyName("end_at")]
    public string EndAt { get; set; }

    [JsonPropertyName("pool_id")]
    public string PoolId { get; set; }

    [JsonPropertyName("up_five_names")]
    public string UpFiveNames { get; set; }

    [JsonPropertyName("up_five_ids")]
    public string UpFiveIds { get; set; }

    [JsonPropertyName("up_four_ids")]
    public string UpFourIds { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class FiveGroupModel
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public FiveGroupData Data { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class VersionPool
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("up_five_role_ids")]
    public List<int> UpFiveRoleIds { get; set; }

    [JsonPropertyName("up_five_weapon_ids")]
    public List<int> UpFiveWeaponIds { get; set; }

    [JsonPropertyName("up_four_role_ids")]
    public List<int> UpFourRoleIds { get; set; }

    [JsonPropertyName("up_four_weapon_ids")]
    public List<int> UpFourWeaponIds { get; set; }

    [JsonPropertyName("start_at")]
    public string StartAt { get; set; }

    [JsonPropertyName("end_at")]
    public string EndAt { get; set; }
}
