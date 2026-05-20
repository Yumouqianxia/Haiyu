using Waves.Core.Contracts.Events;
using Waves.Core.Models.CloudGame;

namespace Waves.Core.Services.CloudGameServices;

public class CloudGameProcessTracker:IAsyncDisposable
{
    private IGameEventSubscription? _subscription;
    private PeriodicTimer _timer;
    private Task _timerTask;
    private bool _isDirty;
    private DateTime lastTime;

    public event Action<CloudGameProcessTracker>? OnProgressChanged;

    public async Task StartTrackingAsync(CloudGameEventPublisher publisher)
    {
        if (publisher == null)
            throw new ArgumentNullException(nameof(publisher));
        _subscription = await publisher.SubscribeAsync(HandleEventAsync).ConfigureAwait(false);
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
        _timerTask = NotifyLoopAsync();
    }

    /// <summary>
    /// 轮询推送数据
    /// </summary>
    /// <returns></returns>
    private async Task NotifyLoopAsync()
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync().ConfigureAwait(false))
            {
                if (_isDirty)
                {
                    _isDirty = false;
                    OnProgressChanged?.Invoke(this);
                }
            }
        }
        catch (Exception) { }
    }

    private async ValueTask HandleEventAsync(CloudMessageArgs args)
    {
        if (args == null)
            return;
        if (this.lastTime == DateTime.MinValue || this.lastTime != args.Time)
        {
            this.lastTime = args.Time;
        }
        if (args.Time < this.lastTime)
        {
            // 避免旧数据覆盖通知
            return;
        }
        

        await ValueTask.CompletedTask;
    }

    

    public async ValueTask DisposeAsync()
    {
        try
        {
            _timer?.Dispose();
            _subscription?.Dispose();
            _subscription = null;
            OnProgressChanged = null;
            if (_timerTask != null)
            {
                await _timerTask;
            }
        }
        catch { }
    }

    
}
