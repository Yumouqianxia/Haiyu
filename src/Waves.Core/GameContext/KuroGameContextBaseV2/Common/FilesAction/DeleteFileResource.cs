namespace Waves.Core.GameContext.Common.FilesAction;

/// <summary>
/// 一次性多文件操作，不能停止
/// </summary>
public class DeleteFileResource : IProgressSetup
{
    public string ProgressName { get; set; }

    public double ProgressValue { get; private set; }

    public bool CanPause => false;

    public bool CanStop => false;

    public Task<object?> ExecuteAsync(bool isSync = false)
    {
        throw new NotImplementedException();
    }

    public void SetParam(Dictionary<string, object> param, IGameEventPublisher<GameContextOutputArgs> gameEventPublisher)
    {
        throw new NotImplementedException();
    }
}