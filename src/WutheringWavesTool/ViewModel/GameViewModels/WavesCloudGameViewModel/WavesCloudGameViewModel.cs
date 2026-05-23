using Haiyu.Services.DialogServices;
using Waves.Api.Models.CloudGame;
using Waves.Core.Common;
using Waves.Core.Contracts.CloudGame;
using Waves.Core.Models.CloudGame;
using Waves.Core.Models.Enums;
using Waves.Core.Services.CloudGameServices;
using Windows.Wdk;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Haiyu.ViewModel.GameViewModels;

public sealed partial class WavesCloudGameViewModel : ViewModelBase
{
    public IKuroCloudGameContext KuroCloudGameContext { get; }
    public IDialogManager DialogManager { get; }
    public IAppContext<App> App { get; }
    public ITipShow TipShow { get; }
    public IWallpaperService WallpaperService { get; }

    [ObservableProperty]
    public partial ObservableCollection<CloudGameLoginSession> Logins { get; set; }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    [ObservableProperty]
    public partial WallDataWrapper WallData { get; set; }

    [ObservableProperty]
    public partial int NodesCount { get; set; }

    [ObservableProperty]
    public partial CloudGameLoginSession SelectLogin { get; set; }

    public WavesCloudGameViewModel(
        IWallpaperService wallpaperService,
        [FromKeyedServices(nameof(Waves.Core.Services.KuroCloudGameContext))]
            IKuroCloudGameContext kuroCloudGameContext,
        [FromKeyedServices(nameof(MainDialogService))] IDialogManager dialogManager,
        IAppContext<App> app,
        ITipShow tipShow
    )
    {
        WallpaperService = wallpaperService;
        KuroCloudGameContext = kuroCloudGameContext;
        DialogManager = dialogManager;
        App = app;
        TipShow = tipShow;
        KuroCloudGameContext.WavesCloudSurivivalService.MessageHandler +=
            WavesCloudSurivivalService_MessageHandler;
        KuroCloudGameContext.CloudGameProcessTracker.OnProgressChanged +=
            CloudGameProcessTracker_OnProgressChanged;
        ;
        RegisterMessager();
    }

    private async void CloudGameProcessTracker_OnProgressChanged(CloudGameProcessTracker obj)
    {
        await App.TryInvokeAsync(async () =>
        {
            if (obj.CoreType == CloudCoreType.QueueUp)
            {
                BottomText = "游戏中";
            }
            if(obj.CoreType == CloudCoreType.OpeningWeb && obj.QueueResult != null)
            {
                CloudGameWindows window = new CloudGameWindows(obj.QueueResult);
                window.Activate();
            }
            if (obj.CoreType == CloudCoreType.QueueDown)
            {
                BottomText = "排队中";
            }
        });
    }

    private async void WavesCloudSurivivalService_MessageHandler(
        object sender,
        CloudMessageArgs session
    )
    {
        if (session.Type == Waves.Core.Models.Enums.CloudCoreType.UserChanged)
        {
            await this.RefreshUserAsync();
        }
    }

    private void RegisterMessager()
    {
        this.Messenger.Register<CloudLoginMessager>(this, CloudLoginMethod);
    }

    private async void CloudLoginMethod(object recipient, CloudLoginMessager message)
    {
        await this.KuroCloudGameContext.WavesCloudSurivivalService.RefreshTaskAsync();
        await Task.Delay(2000);
        await this.RefreshUserAsync();
    }

    public override void Dispose()
    {
        KuroCloudGameContext.WavesCloudSurivivalService.MessageHandler -=
            WavesCloudSurivivalService_MessageHandler;
        KuroCloudGameContext.CloudGameProcessTracker.OnProgressChanged -=
            CloudGameProcessTracker_OnProgressChanged;
        base.Dispose();
    }

    [RelayCommand]
    async Task Loaded()
    {
        try
        {
            IsRefreshing = true;
            WallpaperService.SetMediaForUrl(
                Waves.Core.Models.Enums.WallpaperShowType.Image,
                "https://aki-gm-resources-back.aki-game.com/pv/cg/login.webp"
            );
            await RefreshUserAsync();
            await RefreshCloudNodesAsync();
            IsRefreshing = false;
        }
        catch (Exception ex)
        {
            IsRefreshing = false;
            this.Logger.WriteError(ex.Message + ex.StackTrace);
        }
    }

    [RelayCommand]
    async Task InvokeTask()
    {
        var wallData =
            await this.KuroCloudGameContext.WavesCloudSurivivalService.WavesCloudGameService.GetWalletDataAsync(
                this.SelectLogin,
                this.CTS.Token
            );

        if (wallData == null)
        {
            await TipShow.ShowMessageAsync(wallData.Msg ?? "获取余额失败！", Symbol.Clear);
            return;
        }
        var result = await DialogManager.ShowSelectGameNodeAsync(
            this.SelectLogin.OrginData.Username + this.SelectLogin.OrginData.Sdkuserid
        );

        if (result == null || result.SelectNode == null)
        {
            await TipShow.ShowMessageAsync("请选择节点或节点失效", Symbol.Clear);
            return;
        }
        var qualityOpt = await this.GetOptionsAsync();
        if (qualityOpt == null)
        {
            await TipShow.ShowMessageAsync("构建清晰度失败，日志已记录", Symbol.Clear);
            return;
        }
        _ = Task.Run(async () =>
            await this.KuroCloudGameContext.StartGameAsync(
                this.SelectLogin,
                result.Nodes,
                result.SelectNode,
                qualityOpt,
                this.GetDefaultPayType(wallData.Data)
            )
        );
    }

    public uint GetDefaultPayType(WalletData walletInfo)
    {
        var timeCardinfo =
            DateTimeOffset.FromUnixTimeSeconds(walletInfo.TimeCardInfo.ExpireTimeSeconds)
            - DateTime.Now;
        if (walletInfo.TimeCardInfo is not null && (timeCardinfo.TotalSeconds > 0))
        {
            return (uint)CloudPayType.Pay;
        }
        if (
            walletInfo.ExperienceCardInfo is not null
            && (
                walletInfo.ExperienceCardInfo.Day > 0
                || walletInfo.ExperienceCardInfo.Hour > 0
                || walletInfo.ExperienceCardInfo.Minute > 0
                || walletInfo.ExperienceCardInfo.Second > 0
            )
        )
        {
            return (uint)CloudPayType.Experience; // 体验卡 → Experience(4)
        }
        var freeSeconds = walletInfo.FreeTimeInfo?.LeftSeconds ?? 0;
        var paySeconds = walletInfo.PayTimeInfo?.LeftSeconds ?? 0;
        if (freeSeconds > 0)
            return (uint)CloudPayType.Free;
        if (paySeconds > 0)
            return (uint)CloudPayType.Pay;
        return (uint)CloudPayType.Pay;
    }

    /// <summary>
    /// 构建当前设备最佳的清晰度
    /// </summary>
    /// <returns></returns>
    public async Task<StreamQualityOptions?> GetOptionsAsync()
    {
        try
        {
            var quality = await this.KuroCloudGameContext.GameLocalConfig.GetConfigAsync(
                CloudGameLocalSettingName.QualityType
            );
            var fps = 60;
            var enable = await this.KuroCloudGameContext.GameLocalConfig.GetConfigAsync(
                CloudGameLocalSettingName.EnableImageEnhancement
            );
            var dpi = (int)HwndExtensions.GetDpiForWindow(App.App.MainWindow.GetWindowHandle());
            var area = DisplayArea.Primary.OuterBounds;
            if (
                bool.TryParse(enable, out var enableImage)
                && Enum.TryParse<CloudQualityType>(quality, out var quEnum)
            )
            {
                var mode = new StreamQualityOptions(
                    CloudGameMethod.DefaultBitRate,
                    CloudGameMethod.MinBitRate,
                    fps,
                    area.Width,
                    area.Height,
                    CloudGameMethod.DefaultCodecType,
                    "0",
                    enableImage,
                    dpi,
                    quEnum
                );
                return CloudGameDataHelper.ScaleQualityToPhysical(mode, false);
            }
            else
            {
                Logger.WriteError($"游戏设置内容有错误:{quality}-{enable}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.WriteError($"构建清晰度出错:{ex.Message}");
            return null;
        }
    }

    [RelayCommand]
    async Task OpenSettingsDialog()
    {
        await DialogManager.ShowWavesCloudSettingAsync(GameType.Waves);
    }
}
