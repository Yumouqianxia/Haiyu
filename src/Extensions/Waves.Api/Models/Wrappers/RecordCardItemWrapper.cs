using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using MemoryPack;
using Waves.Api.Models.CloudGame;

namespace Waves.Api.Models.Wrappers;

[MemoryPackable]
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.PublicProperties
    | DynamicallyAccessedMemberTypes.PublicMethods
)]
public partial class RecordCardItemWrapper : ObservableObject
{
    [JsonConstructor]
    [MemoryPackConstructor]
    public RecordCardItemWrapper() { }

    public RecordCardItemWrapper(Datum datum)
    {
        CardPoolType = datum.CardPoolType;
        ResourceId = datum.ResourceId;
        QualityLevel = datum.QualityLevel;
        ResourceType = datum.ResourceType;
        Name = datum.Name;
        Count = datum.Count;
        Time = datum.Time;
        var dt = DateTime.Parse(Time);
        RecordTime = dt;
        Day = dt.Day;
        Month = dt.Month;
        Year = dt.Year;
    }

    [ObservableProperty]
    [JsonPropertyName("cardPoolType")]
    public partial string CardPoolType { get; set; }

    [ObservableProperty]
    [JsonPropertyName("resourceId")]
    public partial int ResourceId { get; set; }

    [ObservableProperty]
    [JsonPropertyName("qualityLevel")]
    public partial int QualityLevel { get; set; }

    [ObservableProperty]
    [JsonPropertyName("resourceType")]
    public partial string ResourceType { get; set; }

    [ObservableProperty]
    [JsonPropertyName("name")]
    public partial string Name { get; set; }

    [ObservableProperty]
    [JsonPropertyName("count")]
    public partial int Count { get; set; }

    [ObservableProperty]
    [JsonPropertyName("time")]
    public partial string Time { get; set; }

    [JsonIgnore]
    public DateTime RecordTime { get; set; }

    [JsonIgnore]
    public int Day { get; set; }

    [JsonIgnore]
    public int Month { get; set; }

    [JsonIgnore]
    public int Year { get; set; }

    [MemoryPackOnDeserialized]
    void OnDeserialized()
    {
        if (RecordTime == default && !string.IsNullOrEmpty(Time))
        {
            RecordTime = DateTime.Parse(Time);
            Day = RecordTime.Day;
            Month = RecordTime.Month;
            Year = RecordTime.Year;
        }
    }

}
