using Waves.Core.Settings;

namespace Haiyu.Services;

/// <summary>
/// 简单熔断器
/// </summary>
public class IoCircuitBreaker : IIoCircuitBreaker
{
    private SemaphoreSlim _semaphore;
    private int _currentMax;

    public IoCircuitBreaker(AppSettings appSettings)
    {
        AppSettings = appSettings;
        _currentMax = AppSettings.MaxIoConcurrent;
        _semaphore = new SemaphoreSlim(_currentMax, _currentMax);
    }

    public AppSettings AppSettings { get; }

    public void Release()
    {
        _semaphore.Release();
    }
    public bool TryAcquire()
    {
        if (AppSettings.MaxIoConcurrent != _currentMax)
        {
            _currentMax = AppSettings.MaxIoConcurrent;
            _semaphore = new SemaphoreSlim(_currentMax, _currentMax);
        }
        return _semaphore.Wait(0);
    }
}
