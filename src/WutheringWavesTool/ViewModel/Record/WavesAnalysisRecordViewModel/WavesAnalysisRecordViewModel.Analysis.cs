using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.VoiceCommands;

namespace Haiyu.ViewModel;

partial class WavesAnalysisRecordViewModel
{
    [ObservableProperty]
    public partial ObservableCollection<GameRecordNavigationItem> NavItems { get; set; }

    [ObservableProperty]
    public partial GameRecordNavigationItem SelectNavItem { get; set; }

    /// <summary>
    /// 小保底歪率集合
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<GuaranteRangeWrapper> GuaranteItems { get; set; }

    [ObservableProperty]
    public partial GuaranteRangeWrapper SelectGuarante { get; set; }

    [ObservableProperty]
    public partial double StarAvgValue { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<RecordActivityFiveStarItemWrapper> StarItems { get; set; }

    public void InitNavItems()
    {
        if (NavItems == null)
            NavItems = new();
        NavItems.Clear();
        foreach (var item in this.Cards.Items)
        {
            this.NavItems.Add(item.GetRecordNavItem());
        }
        SelectNavItem = NavItems[0];
    }

    public async Task AnalysisStarAsync()
    {
        if (GuaranteItems == null)
            GuaranteItems = new();
        GuaranteItems.Clear();
        var upIds = RecordHelper.FormatFiveRoleStar(FiveGroup);
        //小保底扇形图
        foreach (var item in this.Cards.Items.Where(x => x.IsFlage()))
        {
            var roleRange = RecordHelper.FormatStartFive(item.Resource, out var lastCount, upIds);
            ArgumentNullException.ThrowIfNull(roleRange.Item1);
            var data = RecordHelper.GetGuaranteedRange(roleRange.Item1);
            GuaranteItems.Add(
                new()
                {
                    NG = data,
                    OK = 100 - data,
                    DisplayName = item.GetRecordNavItem().DisplayName,
                }
            );
            this.SelectGuarante = GuaranteItems[0];
        }
        //称号
        var result = Cards.EvaluateLuck(upIds);
    }

    partial void OnSelectGuaranteChanged(GuaranteRangeWrapper value)
    {
        if (this.GuaranteeChart == null)
            GuaranteeChart = new();
        GuaranteeChart.Clear();
        this.GuaranteeChart = new ObservableCollection<object>()
        {
            new Models.Charts.PipeData() { Name = "中", Values = [value.OK] },
            new Models.Charts.PipeData() { Name = "歪", Values = [value.NG] },
        };
        GuaranteeHeader = $"小保底歪率:{value.OK}";
    }

    partial void OnSelectNavItemChanged(GameRecordNavigationItem value)
    {
        try
        {
            var resources = this.Cards.Items.Where(x => x.PoolType == value.Id).FirstOrDefault();
            ArgumentNullException.ThrowIfNull(resources);
            ArgumentNullException.ThrowIfNull(FiveGroup);
            var temp1 = RecordHelper
                .FormatStartFive(
                    resources.Resource,
                    out var lastCount,
                    RecordHelper.FormatFiveRoleStar(this.FiveGroup)
                )
                .Item1;
            ArgumentNullException.ThrowIfNull(temp1);
            StarItems = temp1.Format(this.AllRole, true).Reverse().ToObservableCollection();
            StarItems.Insert(
                0,
                new RecordActivityFiveStarItemWrapper()
                {
                    Icon = null,
                    Flage = false,
                    Count = lastCount,
                    ShowFlage = Visibility.Collapsed,
                    Name = $"已经垫了{lastCount}发",
                }
            );
            StarAvgValue = Math.Round(StarItems.Select(x => x.Count).Average(), 2);
        }
        catch (Exception)
        {
            return;
        }
    }
}
