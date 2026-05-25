using Waves.Api.Models.CloudGame;
using Waves.Core.Models.Enums;

namespace Waves.Core.Models.CloudGame;

public class CloudMessageArgs
{
    public CloudCoreType Type { get; }
    public string Message { get; internal set; }
    public DateTime Time { get; }
    public BrowserSessionLaunchOptions QueueResult { get; internal set; }
    public bool IsLaunched { get;internal set;  }
    public CloudMessageArgs(CloudCoreType type)
    {
        Type = type;
        Time = DateTime.Now;
    }
}
