namespace Waves.Core.Models;

public struct ProgressData
{
    public GameContextActionType Type; // 区分下载/校验
    public long Bytes;
    public DateTime Timestamp; // 精确时间戳
}