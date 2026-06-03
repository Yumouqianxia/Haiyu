namespace Waves.Core.Contracts.Events.CloudGame;

public interface ICloudGameEventPublisher
{
    /// <summary>
    /// 发布事件（非阻塞）
    /// </summary>
    void Publish(in CloudMessageArgs @event);

    /// <summary>
    /// 订阅事件
    /// </summary>
    ValueTask<IGameEventSubscription> SubscribeAsync(
        Func<CloudMessageArgs, ValueTask> handler
    );
}