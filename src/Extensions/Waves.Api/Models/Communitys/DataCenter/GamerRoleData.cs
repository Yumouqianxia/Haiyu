using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys.DataCenter;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class RoleList
{
    [JsonPropertyName("acronym")]
    public string Acronym { get; set; }

    [JsonPropertyName("attributeId")]
    public int AttributeId { get; set; }

    [JsonPropertyName("attributeName")]
    public string AttributeName { get; set; }

    [JsonPropertyName("breach")]
    public int Breach { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("roleIconUrl")]
    public string RoleIconUrl { get; set; }

    [JsonPropertyName("roleId")]
    public long RoleId { get; set; }

    [JsonPropertyName("roleName")]
    public string RoleName { get; set; }

    [JsonPropertyName("rolePicUrl")]
    public string RolePicUrl { get; set; }

    [JsonPropertyName("starLevel")]
    public int StarLevel { get; set; }

    [JsonPropertyName("weaponTypeId")]
    public int WeaponTypeId { get; set; }

    [JsonPropertyName("weaponTypeName")]
    public string WeaponTypeName { get; set; }
}

public class GamerRoleData
{
    [JsonPropertyName("roleList")]
    public List<RoleList> RoleList { get; set; }

    [JsonPropertyName("showToGuest")]
    public bool ShowToGuest { get; set; }
}
