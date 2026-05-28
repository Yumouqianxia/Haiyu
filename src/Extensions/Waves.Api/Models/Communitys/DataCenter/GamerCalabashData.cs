using System.Text.Json.Serialization;

namespace Waves.Api.Models.Communitys.DataCenter;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class Phantom
{
    [JsonPropertyName("acronym")]
    public string Acronym { get; set; }

    [JsonPropertyName("cost")]
    public int Cost { get; set; }

    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("phantomId")]
    public int PhantomId { get; set; }
}

public class PhantomList
{
    [JsonPropertyName("maxStar")]
    public int MaxStar { get; set; }

    [JsonPropertyName("phantom")]
    public Phantom Phantom { get; set; }

    [JsonPropertyName("star")]
    public int Star { get; set; }
}

public class GamerCalabashData
{
    [JsonPropertyName("baseCatch")]
    public string BaseCatch { get; set; }

    [JsonPropertyName("catchQuality")]
    public int CatchQuality { get; set; }

    [JsonPropertyName("cost")]
    public int Cost { get; set; }

    [JsonPropertyName("isUnlock")]
    public bool IsUnlock { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("maxCount")]
    public int MaxCount { get; set; }

    [JsonPropertyName("phantomList")]
    public List<PhantomList> PhantomList { get; set; }

    [JsonPropertyName("strengthenCatch")]
    public string StrengthenCatch { get; set; }

    [JsonPropertyName("unlockCount")]
    public int UnlockCount { get; set; }
}
