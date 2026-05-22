using System.Text.Json;
using Waves.Api.Models;
using Waves.Api.Models.CloudGame;
using Waves.Core.Common;
using Waves.Core.Contracts;
using Waves.Core.Contracts.CloudGame;
using Waves.Core.Contracts.Events.CloudGame;
using Waves.Core.Models;
using Waves.Core.Models.CloudGame;
using Waves.Core.Services.CloudGameServices;

namespace Waves.Core.Services;

public class KuroCloudGameContext : IKuroCloudGameContext
{
    internal KuroCloudGameContext(WavesCloudSurvivalService cloudGameService)
    {
        WavesCloudSurivivalService = cloudGameService;
    }

    public WavesCloudSurvivalService WavesCloudSurivivalService { get; }

    public ICloudGameEventPublisher CloudGameEventPublisher { get; internal set; }

    public CloudGameProcessTracker CloudGameProcessTracker { get; private set; }

    public GameLocalConfig GameLocalConfig { get; private set; }

    public string GamerConfigPath { get; internal set; }

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
        if (CloudGameProcessTracker != null)
        {
            await CloudGameProcessTracker.DisposeAsync();
            CloudGameProcessTracker = null;
        }
        Directory.CreateDirectory(GamerConfigPath);
        this.GameLocalConfig = new GameLocalConfig(GamerConfigPath + "\\Settings.bat");
        CloudGameProcessTracker = new CloudGameProcessTracker();
        await CloudGameProcessTracker.StartTrackingAsync(this.CloudGameEventPublisher);
    }


    public async Task StartGameAsync(
        CloudGameLoginSession session,
        IEnumerable<CloudGameNode> nodes,
        CloudGameNode node,
        StreamQualityOptions options
    )
    {
        if (false)
        {
            
        }
        var http = CloudGameDataFactory.CreateWebCloudClient(session);
        var bizData = CloudGameDataFactory.CreateCloudBizData(
            nodes.Select(x => new BizCloudNode()
            {
                NodeId = x.NodeList.First().NodeId,
                Result = x.NodeList.First().Delay.ToString(),
            })
        );
        var bizString = JsonSerializer.Serialize(
            bizData,
            CloudGameContext.Default.CloudBizData
        );

        var launchOption = CloudGameDataFactory.BuildLaunchOption(session,options);
        var paramData = CloudGameDataFactory.CreateWebLinkParameters(
            launchOption.Quality.DPI,
            launchOption.Quality.Width,
            launchOption.Quality.Height,
            launchOption.Quality.BitRateMax,
            launchOption.Quality.Fps,
            launchOption.Quality.CodecType,
            bizString,
            nodes,
            node
        );
        var invokeResult =
            await this.WavesCloudSurivivalService.WavesCloudGameService.CommonStartGameAsync(
                http,
                session,
                paramData
            );
        if (invokeResult == null) { }
        if (invokeResult.Code == 1712) { }


    }
}
