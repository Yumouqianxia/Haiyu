namespace Waves.Core.Services.CloudGameServices;

public class CloudGameProcessTracker:IAsyncDisposable
{
    private IGameEventSubscription? _subscription;
    private PeriodicTimer _timer;
    private Task _timerTask;
    private bool _isDirty;
    private DateTime lastTime;

    public CloudCoreType CoreType { get; private set; }
    public BrowserSessionLaunchOptions QueueResult { get; private set; }
    public bool IsQueue { get; private set; }
    public int QueueQty { get; private set; }
    public double QueueWaitSecond { get; private set; }
    public string CurrentRegion { get; private set; }
    public CloudPayType PayType { get; private set; }

    public event Action<CloudGameProcessTracker>? OnProgressChanged;

    public async Task StartTrackingAsync(ICloudGameEventPublisher publisher)
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
        this.CoreType = args.Type;
        this.QueueResult = args.QueueResult;
        this.IsQueue = args.IsQueue;
        this.QueueQty = args.QueueQty;
        this.QueueWaitSecond = args.QueueTime;
        this.CurrentRegion = args.CurrentRegion;
        this.PayType = (CloudPayType)args.PayType;
        _isDirty = true;
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