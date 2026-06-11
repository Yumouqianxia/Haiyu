
using System.Diagnostics.CodeAnalysis;
using MemoryPack;
using Waves.Api.Models.Wrappers;

namespace Waves.Api.Models.Record;

[MemoryPackable]
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.PublicProperties
    | DynamicallyAccessedMemberTypes.PublicMethods
)]
public partial class WavesAnalysisPlayerCard
{
    public DateTime LastUpdater { get; set; }

    public string SessionId { get; set; }

    public IList<WavesAnalysisPlayerCardItem> Items { get; set; }
}

[MemoryPackable]
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.PublicProperties
    | DynamicallyAccessedMemberTypes.PublicMethods
)]
public partial class WavesAnalysisPlayerCardItem
{
    public int PoolType { get; set; }

    public IEnumerable<RecordCardItemWrapper> Resource { get; set; }

}
