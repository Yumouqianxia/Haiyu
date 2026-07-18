namespace Waves.Core.Services.CloudGameServices;

public class CloudGameProcessTracker:TrackerBase<CloudGameProcessTracker, CloudMessageArgs>,IAsyncDisposable
{
    private IGameEventSubscription? _subscription;
    private PeriodicTimer _timer;
    private Task _timerTask;
    private DateTime lastTime;

    public CloudCoreType CoreType { get; private set; }
    public BrowserSessionLaunchOptions QueueResult { get; private set; }
    public bool IsQueue { get; private set; }
    public int QueueQty { get; private set; }
    public double QueueWaitSecond { get; private set; }
    public string CurrentRegion { get; private set; }
    public CloudPayType PayType { get; private set; }

    public override void Invoke() => this.onTrackerHandle?.Invoke(this);

    
    public async override ValueTask HandleEventAsync(CloudMessageArgs args)
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
        this.PayType = (CloudPayType) args.PayType;
        _isDirty = true;
        await ValueTask.CompletedTask;
    }
}
