using System;
using System.Collections.Generic;
using System.Text;
using Waves.Core.Contracts.CloudGame;

namespace Haiyu.ViewModel.DialogViewModels;

public sealed partial class CloudGameSettingViewModel : DialogViewModelBase
{
    [ObservableProperty]
    public ObservableCollection<string> Qualitys { get; set;  }
    public IKuroCloudGameContext CloudGameContext { get; internal set; }
}
