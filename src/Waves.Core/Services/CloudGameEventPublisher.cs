namespace Waves.Core.Services;

/// <summary>
/// 云游戏事件发布器
/// </summary>
public class CloudGameEventPublisher
    : EventPublishBase<CloudMessageArgs>,
        IGameEventPublisher<CloudMessageArgs>,
        IAsyncDisposable,
        IPublisher
{
    public override async ValueTask<IGameEventSubscription> SubscribeAsync(
        Func<CloudMessageArgs, ValueTask> handler
    )
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(CloudGameEventPublisher));
        var id = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        lock (_subscribers)
        {
            _subscribers.Add(
                new SubscriberEntry
                {
                    Id = id,
                    Handler = handler,
                    Cts = cts,
                }
            );
        }
        return new SubscriptionToken<CloudGameEventPublisher>(this, id, cts);
    }
}
