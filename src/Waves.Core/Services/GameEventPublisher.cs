namespace Waves.Core.Services;

public sealed class GameEventPublisher
    : EventPublishBase<GameContextOutputArgs>,
        IGameEventPublisher,
        IAsyncDisposable,
        IPublisher
{
    private async Task DispatchEventsAsync()
    {
        try
        {
            await foreach (var @event in Channel.Reader.ReadAllAsync(CTS.Token))
            {
                SubscriberEntry[] subscribersSnapshot;
                lock (_subscribers)
                {
                    subscribersSnapshot = _subscribers.Where(s => !s.IsDisposed).ToArray();
                }
                if (subscribersSnapshot.Length > 0)
                {
                    var tasks = subscribersSnapshot
                        .Select(s => SafelyHandleEvent(s.Handler, @event, s.Cts.Token))
                        .Select(x => x.AsTask())
                        .ToArray();
                    await Task.WhenAll(tasks);
                }
            }
        }
        catch (OperationCanceledException) when (CTS.Token.IsCancellationRequested) { }
    }

    private static async ValueTask SafelyHandleEvent(
        Func<GameContextOutputArgs, ValueTask> handler,
        GameContextOutputArgs @event,
        CancellationToken token
    )
    {
        try
        {
            await handler(@event);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // 订阅者主动取消，正常
        }
        catch (Exception)
        {
            // 订阅者异常，记录但不影响其他订阅者
            // 需要注入 ILogger
        }
    }

    public void Unsubscribe(Guid id)
    {
        lock (_subscribers)
        {
            var subscriber = _subscribers.FirstOrDefault(s => s.Id == id);
            if (subscriber != null)
            {
                subscriber.IsDisposed = true;
                if (!subscriber.Cts.IsCancellationRequested)
                {
                    subscriber.Cts.Cancel();
                    subscriber.Cts.Dispose();
                }
                _subscribers.Remove(subscriber);
            }
        }
    }

    public override async ValueTask<IGameEventSubscription> SubscribeAsync(
        Func<GameContextOutputArgs, ValueTask> handler
    )
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(GameEventPublisher));
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
        return new SubscriptionToken<GameEventPublisher>(this, id, cts);
    }

    
}

public sealed class SubscriptionToken<Publisher> : IGameEventSubscription
        where Publisher : IPublisher
{
    private readonly Publisher _publisher;
    private readonly Guid _id;
    private readonly CancellationTokenSource _cts;
    private bool _isDisposed;

    public SubscriptionToken(Publisher publisher, Guid id, CancellationTokenSource cts)
    {
        _publisher = publisher;
        _id = id;
        _cts = cts;
    }

    public bool IsActive => !_isDisposed && !_cts.IsCancellationRequested;

    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        _cts.Cancel();
        _cts.Dispose();
        _publisher.Unsubscribe(_id);
    }
}
