namespace Waves.Core.Models.CloudGame;

public class CloudMessageArgs
{
    internal CloudMessageArgs() { }

    public CloudCoreType Type { get; }
    
    public string Message { get; internal set; }
    
    public DateTime Time { get; }
    
    public BrowserSessionLaunchOptions QueueResult { get; internal set; }

    public bool IsLaunched { get;internal set;  }

    /// <summary>
    /// 是否排队
    /// </summary>
    public bool IsQueue { get; internal set; }

    /// <summary>
    /// 排队人数
    /// </summary>
    public int QueueQty { get; internal set; }

    /// <summary>
    /// 预计排队时间
    /// </summary>
    public double QueueTime { get; internal set; }



    /// <summary>
    /// 当前使用节点
    /// </summary>
    public string CurrentRegion { get; internal set; }

    /// <summary>
    /// 当前排队通道
    /// </summary>
    public uint PayType { get; internal set; }

    public CloudMessageArgs(CloudCoreType type)
    {
        Type = type;
        Time = DateTime.Now;
    }
}