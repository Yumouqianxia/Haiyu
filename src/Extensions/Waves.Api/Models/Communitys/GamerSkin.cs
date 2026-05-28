using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys;

public class RoleSkinList
{
    [JsonPropertyName("acronym")]
    public string Acronym { get; set; }

    [JsonPropertyName("isAddition")]
    public bool IsAddition { get; set; }

    [JsonPropertyName("picUrl")]
    public string PicUrl { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("quality")]
    public int Quality { get; set; }

    [JsonPropertyName("qualityName")]
    public string QualityName { get; set; }

    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }

    [JsonPropertyName("roleName")]
    public string RoleName { get; set; }

    [JsonPropertyName("skinIcon")]
    public string SkinIcon { get; set; }

    [JsonPropertyName("skinId")]
    public int SkinId { get; set; }

    [JsonPropertyName("skinName")]
    public string SkinName { get; set; }
}

public class GamerSkin
{
    [JsonPropertyName("roleSkinList")]
    public List<RoleSkinList> RoleSkinList { get; set; }

    [JsonPropertyName("showToGuest")]
    public bool ShowToGuest { get; set; }

    [JsonPropertyName("weaponSkinList")]
    public List<WeaponSkinList> WeaponSkinList { get; set; }
}

public class WeaponSkinList
{
    [JsonPropertyName("isAddition")]
    public bool IsAddition { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("quality")]
    public int Quality { get; set; }

    [JsonPropertyName("qualityName")]
    public string QualityName { get; set; }

    [JsonPropertyName("skinIcon")]
    public string SkinIcon { get; set; }

    [JsonPropertyName("skinId")]
    public int SkinId { get; set; }

    [JsonPropertyName("skinName")]
    public string SkinName { get; set; }

    [JsonPropertyName("weaponTypeIcon")]
    public string WeaponTypeIcon { get; set; }

    [JsonPropertyName("weaponTypeId")]
    public int WeaponTypeId { get; set; }

    [JsonPropertyName("weaponTypeName")]
    public string WeaponTypeName { get; set; }
}
