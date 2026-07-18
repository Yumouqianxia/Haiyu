namespace Waves.Core.Contracts.CloudGame;

/// <summary>
/// 云游戏通用接口
/// </summary>
public interface ICloudGameContext
{

    public IGameEventPublisher<CloudMessageArgs> GameEventPublisher { get; }
}
