using System;
using System.Collections.Generic;
using System.Text;
using LiveChartsCore;
using Haiyu.Models.Charts;
namespace Haiyu.ViewModel;

partial class WavesAnalysisRecordViewModel
{
    #region 小保底歪率饼图
    [ObservableProperty]
    public partial string GuaranteeHeader { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<object> GuaranteeChart { get; set; }
    #endregion
}
