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
            StarItems = temp1.Format(this.AllRole, true)
                        .Reverse()
                        .ToObservableCollection();
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
