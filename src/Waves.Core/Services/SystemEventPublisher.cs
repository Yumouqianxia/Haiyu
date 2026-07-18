namespace Waves.Core.Services;

/// <summary>
/// 系统消息
/// </summary>
public class SystemEventPublisher : EventPublishBase<SystemMessagerModel>,
        IGameEventPublisher<SystemMessagerModel>,
        IAsyncDisposable,
        IPublisher
{
    public override async ValueTask<IGameEventSubscription> SubscribeAsync(
       Func<SystemMessagerModel, ValueTask> handler
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
        return new SubscriptionToken<SystemEventPublisher>(this, id, cts);
    }

}
