using Waves.Core.Models.Enums;

namespace Waves.Core.Models.CloudGame;

public class CloudMessageArgs
{
    public CloudCoreType Type { get; }
    public string Message { get; internal set; }
    public DateTime Time { get; }
    public CloudMessageArgs(CloudCoreType type)
    {
        Type = type;
        Time = DateTime.Now;
    }
}
