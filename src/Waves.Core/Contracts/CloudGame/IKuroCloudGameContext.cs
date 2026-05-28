using System;
using System.Collections.Generic;
using System.Text;
using Waves.Api.Models.CloudGame;
using Waves.Core.Contracts.Events.CloudGame;
using Waves.Core.Models;
using Waves.Core.Models.CloudGame;
using Waves.Core.Models.Enums;
using Waves.Core.Services;
using Waves.Core.Services.CloudGameServices;

namespace Waves.Core.Contracts.CloudGame;

/// <summary>
/// 云库洛游戏上下文接口
/// </summary>
public interface IKuroCloudGameContext
{
    WavesCloudSurvivalService WavesCloudSurivivalService { get; }
    ICloudGameEventPublisher CloudGameEventPublisher { get; }
    CloudGameProcessTracker CloudGameProcessTracker { get; }
    GameLocalConfig GameLocalConfig { get; }
    
    Task InitAsync();

    Task StartGameAsync(
        CloudGameLoginSession session,
        IEnumerable<CloudGameNode> nodes,
        CloudGameNode node,
        StreamQualityOptions options,
        uint payType
    );

    Task StopQueueAsync();

    Task<KuroCLoudGameCoreState> GetCloudStateAsync();


    /// <summary>
    /// 取消当前活动排队
    /// </summary>
    /// <returns></returns>
    Task ClearActiveAsync();

    void ClearWindow();
    void SetGameingWindow(nint handle, string titleKey);
}
