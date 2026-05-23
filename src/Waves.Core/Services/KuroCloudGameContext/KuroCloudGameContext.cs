using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Text.Json;
using Waves.Api.Models;
using Waves.Api.Models.CloudGame;
using Waves.Core.Common;
using Waves.Core.Contracts;
using Waves.Core.Contracts.CloudGame;
using Waves.Core.Contracts.Events.CloudGame;
using Waves.Core.Models;
using Waves.Core.Models.CloudGame;
using Waves.Core.Models.Enums;
using Waves.Core.Services.CloudGameServices;
using static System.Collections.Specialized.BitVector32;

namespace Waves.Core.Services;

public class KuroCloudGameContext : IKuroCloudGameContext
{
    internal KuroCloudGameContext(WavesCloudSurvivalService cloudGameService)
    {
        WavesCloudSurivivalService = cloudGameService;
    }

    /// <summary>
    /// 正在运行的游戏窗口句柄
    /// </summary>
    private uint GameingWindow { get; set; }

    public WavesCloudSurvivalService WavesCloudSurivivalService { get; }

    public ICloudGameEventPublisher CloudGameEventPublisher { get; internal set; }

    public CloudGameProcessTracker CloudGameProcessTracker { get; private set; }

    public GameLocalConfig GameLocalConfig { get; private set; }

    public string GamerConfigPath { get; internal set; }
    #region 排队参数
    public CancellationTokenSource? queqeCTS = null;
    public System.Threading.PeriodicTimer? _queqeTimer;
    #endregion

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
        StreamQualityOptions options,
        uint payType
    )
    {
        var http = CloudGameDataHelper.CreateWebCloudClient(session);
        var bizData = CloudGameDataHelper.CreateCloudBizData(
            nodes.Select(x => new BizCloudNode()
            {
                NodeId = x.NodeList.First().NodeId,
                Result = x.NodeList.First().Delay.ToString(),
            })
        );
        var bizString = JsonSerializer.Serialize(bizData, CloudGameContext.Default.CloudBizData);
        var launchOption = CloudGameDataHelper.BuildLaunchOption(session, options);
        var paramData = CloudGameDataHelper.CreateWebLinkParameters(
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
                paramData,
                payType
            );
        if (invokeResult == null)
        {
            this.CloudGameEventPublisher.Publish(new(CloudCoreType.QueueDown));
            await Task.Delay(1000);
            //启动Web串流，通知外部ViewModel接受Session，进行Web启动
            this.CloudGameEventPublisher.Publish(new(CloudCoreType.OpeningWeb));
            return;
        }
        if(invokeResult != null && invokeResult.Code == 0)
        {
            var option = launchOption.Clone();
            this.CloudGameEventPublisher.Publish(new(CloudCoreType.QueueDown));
            if(queqeCTS!=null)
                await queqeCTS.CancelAsync();
            option.IsComplete = true;
            option.StreamOptions = new CloudGameStreamSession()
            {
                TenantKey = CloudGameDataHelper.WelinkTenantKey,
                ScriptUrl = CloudGameMethod.WelinkScriptUrl,
                GameId = CloudGameDataHelper.WelinkGameId,
                StartParameters = paramData,
                RegionName = invokeResult.Data.RegionName,
                DispatchMessage = invokeResult.Data.DispatchResult.DispatchMsg,
                SessionKey = "1-",
            };
            this.CloudGameEventPublisher.Publish(
                new(CloudCoreType.OpeningWeb) { QueueResult = option }
            );
            return;
        }
        if (invokeResult.Code == 1712)
        {
            this.CloudGameEventPublisher.Publish(new(CloudCoreType.QueueUp));
            if (queqeCTS != null)
                await queqeCTS.CancelAsync();
            if (_queqeTimer != null)
                _queqeTimer.Dispose();
            queqeCTS = new();
            _queqeTimer = new PeriodicTimer(System.TimeSpan.FromSeconds(2));
            _ = Task.Run(async () => await QueqeTask(session, paramData, launchOption));
        }
    }

    private async Task QueqeTask(
        CloudGameLoginSession session,
        WelinkStartParameters welinkParam,
        BrowserSessionLaunchOptions launchOption
    )
    {
        var option = launchOption.Clone();
        if (_queqeTimer == null || _queqeTimer.Period == TimeSpan.Zero || queqeCTS == null)
        {
            this.CloudGameEventPublisher.Publish(
                new(CloudCoreType.Message) { Message = "排队计时器异常！请重试进入游戏" }
            );
            return;
        }
        try
        {
            while (await _queqeTimer.WaitForNextTickAsync(queqeCTS.Token))
            {
                if (queqeCTS.IsCancellationRequested)
                {
                    this.CloudGameEventPublisher.Publish(new(CloudCoreType.None));
                    return;
                }
                var http = CloudGameDataHelper.CreateWebCloudClient(session);
                var queueResult =
                    await this.WavesCloudSurivivalService.WavesCloudGameService.CommonQueueInfoAsync(
                        http,
                        session
                    );
                if (queueResult == null)
                    continue;
                if (queueResult.Code == 0 && queueResult.Data?.Code == 200)
                {
                    this.CloudGameEventPublisher.Publish(new(CloudCoreType.QueueDown));
                    await queqeCTS.CancelAsync();
                    option.IsComplete = true;
                    option.StreamOptions = new CloudGameStreamSession()
                    {
                        TenantKey = CloudGameDataHelper.WelinkTenantKey,
                        ScriptUrl = CloudGameMethod.WelinkScriptUrl,
                        GameId = CloudGameDataHelper.WelinkGameId,
                        StartParameters = welinkParam,
                        RegionName = queueResult.Data.RegionName,
                        DispatchMessage = queueResult.Data.DispatchResult.DispatchMsg,
                        SessionKey = "1-",
                    };
                    this.CloudGameEventPublisher.Publish(
                        new(CloudCoreType.OpeningWeb) { QueueResult = option }
                    );
                    return;
                }
                if (queueResult.Code == 1712)
                {
                    option.IsComplete = false;
                    this.CloudGameEventPublisher.Publish(
                        new(CloudCoreType.QueueDown) { QueueResult = option }
                    );
                }
                if (queueResult.Code == 1724)
                {
                    option.IsComplete = false;
                    this.CloudGameEventPublisher.Publish(
                        new(CloudCoreType.Message)
                        {
                            Message = queueResult.Msg ?? "排队异常，请重试",
                            QueueResult = option,
                        }
                    );
                    await queqeCTS.CancelAsync();
                    return;
                }
            }
        }
        catch (OperationCanceledException) { }
    }
}
