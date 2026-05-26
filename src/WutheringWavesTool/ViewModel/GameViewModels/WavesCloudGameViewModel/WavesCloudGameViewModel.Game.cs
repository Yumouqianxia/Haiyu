using System;
using System.Collections.Generic;
using System.Text;

namespace Haiyu.ViewModel.GameViewModels;

partial class WavesCloudGameViewModel
{
    [ObservableProperty]
    public partial string BottomText { get; set; } = "准备就绪";

    [ObservableProperty]
    public partial string StartGameText { get; set; } = "进入游戏";

    
}