using CommunityToolkit.Mvvm.ComponentModel;

namespace Haiyu.Plugin.Models;

public partial class DisplayVersionInfo:ObservableObject
{
    [ObservableProperty]
    public partial string Version { get; set; }

    [ObservableProperty]
    public partial string UpdateAt { get; set;  }

    [ObservableProperty]
    public partial long Size { get; set;  }

    [ObservableProperty]
    public partial string HelpLink { get; set;  }

    [ObservableProperty]
    public partial string DownloadLink { get; set; }

    /// <summary>
    /// 是否显示跳过
    /// </summary>
    public bool IsApply { get; set; }
}
