using Waves.Core.Contracts.Events;
using Waves.Core.Contracts.Events.CloudGame;
using Waves.Core.Models;
using Waves.Core.Models.CloudGame;

namespace Waves.Core.Services;

/// <summary>
/// 云游戏事件发布器
/// </summary>
public class CloudGameEventPublisher
    : EventPublishBase<CloudMessageArgs>,
        ICloudGameEventPublisher,
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
        Func<CloudMessageArgs, ValueTask> handler,
        CloudMessageArgs @event,
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
