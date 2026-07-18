using CommunityToolkit.Mvvm.ComponentModel;

namespace Waves.Api.Models.Record;

public partial class GuaranteRangeWrapper
{
    public double OK { get; set; }

    public double NG { get; set; }

    public string DisplayName { get; set; }

    public int ConsecutiveLoss { get; set; }

    public string GuaranteeStatus { get; set; }
}
