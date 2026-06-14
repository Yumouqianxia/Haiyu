namespace Waves.Core.Contracts.Events;

/// <summary>
/// 游戏事件发布者接口
/// </summary>
public interface IGameEventPublisher<TValue>
{
    /// <summary>
    /// 发布事件（非阻塞）
    /// </summary>
    void Publish(in TValue @event);

    /// <summary>
    /// 订阅事件
    /// </summary>
    ValueTask<IGameEventSubscription> SubscribeAsync(
        Func<TValue, ValueTask> handler
    );
}
