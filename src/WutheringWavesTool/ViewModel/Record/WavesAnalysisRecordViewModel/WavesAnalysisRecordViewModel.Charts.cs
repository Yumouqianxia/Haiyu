using System;
using System.Collections.Generic;
using System.Text;
using Haiyu.Models.Charts;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
namespace Haiyu.ViewModel;

partial class WavesAnalysisRecordViewModel
{
    #region 小保底歪率饼图
    [ObservableProperty]
    public partial string GuaranteeHeader { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<object> GuaranteeChart { get; set; }
    #endregion

    #region 出货占比饼图
    [ObservableProperty]
    public partial ObservableCollection<object> StarRatioChart { get; set; }
    #endregion

    #region 各卡池抽数饼图
    [ObservableProperty]
    public partial ObservableCollection<object> PoolChart { get; set; }
    #endregion

    #region 每日抽数柱状图
    [ObservableProperty]
    public partial ObservableCollection<DateTimePoint> TimeLineChart { get; set; }

    public Func<DateTime, string> TimeLineFormatter { get; } =
        date => date.ToString("MM/dd");
    #endregion

    public Func<DateTime, string> Formatter { get; set; } =
        date => date.ToString("yyyy-MM-dd");
}
