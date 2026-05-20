using System.Text.Json;
using Waves.Api.Models;
using Waves.Api.Models.CloudGame;
using Waves.Core.Common;
using Waves.Core.Contracts;
using Waves.Core.Contracts.CloudGame;
using Waves.Core.Models.CloudGame;
using Waves.Core.Services.CloudGameServices;

namespace Waves.Core;

public class KuroCloudGameContext : IKuroCloudGameContext
{
    public KuroCloudGameContext(WavesCloudSurvivalService cloudGameService)
    {
        WavesCloudSurivivalService = cloudGameService;
    }

    public WavesCloudSurvivalService WavesCloudSurivivalService { get; }

    public async Task InitAsync()
    {
        if (WavesCloudSurivivalService.IsRuning)
        {
            await WavesCloudSurivivalService.StopAsync();
        }
        else
        {
            await WavesCloudSurivivalService.StartAsync();
        }
    }

    public async Task StartGameAsync(
        CloudGameLoginSession session,
        int dpi,
        IEnumerable<CloudGameNode> nodes,
        CloudGameNode node
    )
    {
        var http = CloudGameDataFactory.CreateWebCloudClient(session);
        var bizData = CloudGameDataFactory.CreateCloudBizData(
            nodes.Select(x => new BizCloudNode()
            {
                NodeId = x.NodeList.First().NodeId,
                Result = x.NodeList.First().Delay.ToString(),
            })
        );
        var bizString = JsonSerializer.Serialize(bizData, CloudGameContext.Default.CloudBizData);
        var launchOption = CloudGameDataFactory.BuildLaunchOption(session, dpi);
        var paramData = CloudGameDataFactory.CreateWebLinkParameters(
            dpi,
            launchOption.Quality.Width,
            launchOption.Quality.Height,
            launchOption.Quality.BitRateMax,
            launchOption.Quality.Fps,
            launchOption.Quality.CodecType,
            bizString,
            nodes,
            node
        );
        var invokeResult =  await this.WavesCloudSurivivalService.WavesCloudGameService.CommonStartGameAsync(http, session, paramData);
        if(invokeResult == null)
        {

        }
        if(invokeResult.Code == 1712)
        {

        }
    }
}
