namespace Waves.Core.Contracts.Events;

public delegate void OnTrakcerChanged<TValue>(TValue value);

public abstract class TrackerBase<ProgressArgs, EventArgs> : IAsyncDisposable
    where ProgressArgs : class
{
    protected IGameEventSubscription? _subscription;
    protected PeriodicTimer? _timer;
    protected Task? _timerTask;
    protected bool _isDirty;
    protected SynchronizationContext? _syncContext;

    internal OnTrakcerChanged<ProgressArgs>? onTrackerHandle;

    public event OnTrakcerChanged<ProgressArgs>? OnProgressChanged
    {
        add => onTrackerHandle += value;
        remove => onTrackerHandle -= value;
    }

    public async Task StartTrackingAsync(IGameEventPublisher<EventArgs> publisher)
    {
        if (publisher == null)
            throw new ArgumentNullException(nameof(publisher));
        _syncContext = SynchronizationContext.Current;
        _subscription = await publisher.SubscribeAsync(HandleEventAsync).ConfigureAwait(false);
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
        _timerTask = NotifyLoopAsync();
    }

    public abstract ValueTask HandleEventAsync(EventArgs args);

    private async Task NotifyLoopAsync()
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync().ConfigureAwait(false))
            {
                if (_isDirty)
                {
                    _isDirty = false;
                    if (_syncContext != null)
                        _syncContext.Post(_ => Invoke(), null);
                    else
                        Invoke();
                }
            }
        }
        catch (Exception) { }
    }

    public abstract void Invoke();

    public async ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        _subscription?.Dispose();
        _subscription = null;
        if (_timerTask != null)
            await _timerTask;
        await OnVirualDispose();
    }

    public virtual async Task OnVirualDispose() { }
}
