namespace Waves.Core.Models.Enums;

/// <summary>
/// 云游戏核心消息类型
/// </summary>
public enum CloudCoreType:uint
{
    /// <summary>
    /// 正在请求开始
    /// </summary>
    RequestCloud = 1,
    /// <summary>
    /// 正在排队
    /// </summary>
    QueueUp =2,
    /// <summary>
    /// 排队结束
    /// </summary>
    QueueDown = 4,
    /// <summary>
    /// 正在打开串流窗口
    /// </summary>
    OpeningWeb = 5,
    /// <summary>
    /// 正在游戏中
    /// </summary>
    InGameing = 6,
    /// <summary>
    /// 有错误
    /// </summary>
    ErrorFlage = 7,
    /// <summary>
    /// 账户变动
    /// </summary>
    UserChanged = 8,
    /// <summary>
    /// 消息通知
    /// </summary>
    Message = 9
}
