using System;
using System.Collections.Generic;
using System.Text;
using Waves.Api.Models.CloudGame;
using Waves.Core.Models.CloudGame;
using Waves.Core.Services.CloudGameServices;

namespace Waves.Core.Contracts.CloudGame;

/// <summary>
/// 云库洛游戏上下文接口
/// </summary>
public interface IKuroCloudGameContext
{
    WavesCloudSurvivalService WavesCloudSurivivalService { get; }

    public Task InitAsync();

    public Task StartGameAsync(
        CloudGameLoginSession session,
        int dpi,
        IEnumerable<CloudGameNode> nodes,
        CloudGameNode node
    );
}
