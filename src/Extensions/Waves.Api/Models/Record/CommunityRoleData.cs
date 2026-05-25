using System.Text.Json.Serialization;

namespace Waves.Api.Models.Record;

public class Prop
{
    [JsonPropertyName("hp")]
    public int Hp { get; set; }

    [JsonPropertyName("att")]
    public int Att { get; set; }

    [JsonPropertyName("def")]
    public int Def { get; set; }

    [JsonPropertyName("cc%")]
    public int Cc { get; set; }

    [JsonPropertyName("cd%")]
    public int Cd { get; set; }

    [JsonPropertyName("ee%")]
    public int Ee { get; set; }

    [JsonPropertyName("em")]
    public int Em { get; set; }
}

public class CommunityRoleData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("star")]
    public int Star { get; set; }

    [JsonPropertyName("tag")]
    public List<Tag> Tag { get; set; }

    [JsonPropertyName("weaponType")]
    public List<string> WeaponType { get; set; }

    [JsonPropertyName("iconhalf")]
    public string Iconhalf { get; set; }

    [JsonPropertyName("iconbig")]
    public string Iconbig { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("typeIcon")]
    public string TypeIcon { get; set; }

    [JsonPropertyName("en")]
    public string En { get; set; }

    [JsonPropertyName("birthday")]
    public string Birthday { get; set; }

    [JsonPropertyName("sex")]
    public string Sex { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("influence")]
    public string Influence { get; set; }

    [JsonPropertyName("prop")]
    public Prop Prop { get; set; }

    [JsonPropertyName("talentName")]
    public string TalentName { get; set; }
}

public class Tag
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("des")]
    public string Des { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }
}
