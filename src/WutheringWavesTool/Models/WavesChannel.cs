namespace Haiyu.Models;

public enum WavesChannel
{
    Unknown = 0,
    Official = 1,
    Bilibili = 2,
    WeGame = 3,
}

public enum WavesLaunchMode
{
    Haiyu = 0,
    WeGame = 1,
}

public sealed class WavesChannelOption
{
    public WavesChannel Channel { get; set; }

    public string Display { get; set; }

    public string Tag { get; set; }
}

public sealed class WavesLaunchModeOption
{
    public WavesLaunchMode Mode { get; set; }

    public string Display { get; set; }

    public string Description { get; set; }
}

public sealed class WavesChannelStatus
{
    public WavesChannel CurrentChannel { get; set; }

    public bool ActivePackageComplete { get; set; }

    public bool SelectedBackupExists { get; set; }

    public string Message { get; set; }
}
