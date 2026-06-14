

namespace Haiyu.Models.Wrapper;

public partial class RecordHealderWrapper:ObservableObject
{
    /// <summary>
    /// 抽卡总数
    /// </summary>
    [ObservableProperty]
    public partial int AllTotal { get; set; }

    /// <summary>
    /// 称号
    /// </summary>
    [ObservableProperty]
    public partial string Designation { get; set; }

    /// <summary>
    /// 双金次数
    /// </summary>
    [ObservableProperty]
    public partial int DoubleCount { get; set; }

    /// <summary>
    /// 歪了多少次
    /// </summary>
    [ObservableProperty]
    public partial int CrookedTotal { get; set; }

    /// <summary>
    /// 总出金
    /// </summary>
    [ObservableProperty]
    public partial int AllStarTotal { get; set; }

    /// <summary>
    /// 综合评分
    /// </summary>
    [ObservableProperty]
    public partial double Score { get; set; }

    /// <summary>
    /// 平均出金抽数
    /// </summary>
    [ObservableProperty]
    public partial double AvgPulls { get; set; }

    /// <summary>
    /// 实际出金率(%)
    /// </summary>
    [ObservableProperty]
    public partial double ActualFiveStarRate { get; set; }

    /// <summary>
    /// 抽卡跨度天数
    /// </summary>
    [ObservableProperty]
    public partial int Days { get; set; }
}
