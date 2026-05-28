namespace Waves.Core.Models.CloudGame;

public class KuroCLoudGameCoreState
{
    public nint? WindowHandle { get; internal set; }
    public bool IsQueue { get; internal set; }
    public int QueueQty { get; internal set; }
    public string Region { get; internal set; }
    public int QueueWaitTime { get; internal set; }
    public string WindowTitleKey { get; internal set; }
}
