using LiveChartsCore.SkiaSharpView.Extensions;
using Waves.Core.Common;
using Waves.Core.Models.CoreApi;
using Waves.Core.Models.Enums;
using Windows.ApplicationModel.DataTransfer;

namespace Haiyu.ViewModel.GameViewModels.GameContexts;

public partial class WavesV2GameContextViewModel : KuroGameContextViewModelV2
{
    private const string WavesSelectedChannelKey = "WavesSelectedChannel";
    private const string WavesLaunchModeKey = "WavesLaunchMode";
    private const string WeGameWutheringWavesProtocol = "wegame://action=StartFor&game_id=2002137/";

    private readonly IWavesChannelService _wavesChannelService;
    private readonly IPickersService _pickersService;

    public WavesV2GameContextViewModel(
        IAppContext<App> appContext,
        ITipShow tipShow,
        IWavesChannelService wavesChannelService,
        IPickersService pickersService
    )
        : base(appContext, tipShow)
    {
        _wavesChannelService = wavesChannelService;
        _pickersService = pickersService;
        WeakReferenceMessenger.Default.Register<LocalGameRefreshBindUser>(
            this,
            LocalGameRefreshBindUserMethod
        );
    }

    private async void LocalGameRefreshBindUserMethod(
        object recipient,
        LocalGameRefreshBindUser message
    )
    {
        if (message.data?.PlayerItem?.Type != GameType.Waves)
        {
            return;
        }
        await this.RefreshLocalGameUser(message.data);
    }

    public override void DisposeAfter()
    {
        if (this.Contents != null)
            this.Contents.Clear();
        if (this.Activity != null)
        {
            this.Activity.Contents.Clear();
            this.Activity.Contents = null;
        }
        if (this.Notice != null)
        {
            this.Notice.Contents.Clear();
            this.Notice.Contents = null;
        }
        if (this.News != null)
        {
            this.News.Contents.Clear();
            this.News.Contents = null;
        }
        if (this.SlideShows != null)
        {
            this.SlideShows.Clear();
            this.SlideShows = null;
        }
    }

    public override Task LoadAfter()
    {
        return LoadChannelStateAsync();
    }

    public override GameType GameType => GameType.Waves;

    [ObservableProperty]
    public partial ObservableCollection<WavesChannelOption> ChannelOptions { get; set; } =
        new()
        {
            new WavesChannelOption { Channel = WavesChannel.Official, Display = "官方", Tag = "A1381" },
            new WavesChannelOption { Channel = WavesChannel.Bilibili, Display = "B服", Tag = "A1421" },
            new WavesChannelOption { Channel = WavesChannel.WeGame, Display = "WeGame", Tag = "A1440" },
        };

    [ObservableProperty]
    public partial WavesChannelOption SelectedChannelOption { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<WavesLaunchModeOption> LaunchModeOptions { get; set; } =
        new()
        {
            new WavesLaunchModeOption
            {
                Mode = WavesLaunchMode.WeGame,
                Display = "通过 WeGame",
                Description = "优先保留 QQ 钱包/Q币充值链路",
            },
            new WavesLaunchModeOption
            {
                Mode = WavesLaunchMode.Haiyu,
                Display = "Haiyu 直启",
                Description = "直启不保证 WeGame 支付链路",
            },
        };

    [ObservableProperty]
    public partial WavesLaunchModeOption SelectedLaunchModeOption { get; set; }

    [ObservableProperty]
    public partial string ChannelStatusText { get; set; } = "渠道状态未检测";

    [ObservableProperty]
    public partial string ChannelStatusShortText { get; set; } = "当前：未知";

    [ObservableProperty]
    public partial bool IsChannelOperationRunning { get; set; }

    [ObservableProperty]
    public partial bool IsWeGameChannelSelected { get; set; }

    async partial void OnSelectedChannelOptionChanged(WavesChannelOption value)
    {
        if (value == null || GameContext == null)
        {
            return;
        }

        IsWeGameChannelSelected = value.Channel == WavesChannel.WeGame;
        await GameContext.GameLocalConfig.SaveConfigAsync(WavesSelectedChannelKey, value.Channel.ToString());
        await SelectDefaultLaunchModeAsync(value.Channel);
        await RefreshChannelStatusAsync();
    }

    async partial void OnSelectedLaunchModeOptionChanged(WavesLaunchModeOption value)
    {
        if (value == null || GameContext == null)
        {
            return;
        }

        await GameContext.GameLocalConfig.SaveConfigAsync(WavesLaunchModeKey, value.Mode.ToString());
    }

    [ObservableProperty]
    public partial ObservableCollection<Slideshow> SlideShows { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<string> Tabs { get; set; } =
        new ObservableCollection<string>() { "活动", "公告", "新闻" };

    [ObservableProperty]
    public partial string SelectTab { get; set; }


    [ObservableProperty]
    public partial int SwatchIndex { get; set; }

    [ObservableProperty]
    public partial KRSDKLauncherCacheWrapper SelectDisplayerLocalUser { get; set; }

    partial void OnSelectTabChanged(string value)
    {
        if (value == null)
        {
            Contents.Clear();
            return;
        }
        if (value == Tabs[0])
        {
            Contents = Activity.Contents.ToObservableCollection();
        }
        else if (value == Tabs[1])
        {
            Contents = Notice.Contents.ToObservableCollection();
        }
        else if (value == Tabs[2])
        {
            Contents = News.Contents.ToObservableCollection();
        }
    }

    #region Datas
    public Notice Notice { get; private set; }
    public News News { get; private set; }
    public Waves.Api.Models.Activity Activity { get; private set; }

    [ObservableProperty]
    public partial Visibility PlayerCardVisibility { get; set; }

    #endregion

    private async Task LoadChannelStateAsync()
    {
        if (GameContext == null)
        {
            return;
        }

        var selectedChannel = await GameContext.GameLocalConfig.GetConfigAsync(WavesSelectedChannelKey);
        if (!Enum.TryParse<WavesChannel>(selectedChannel, out var channel) || channel == WavesChannel.Unknown)
        {
            channel = WavesChannel.Official;
        }

        SelectedChannelOption =
            ChannelOptions.FirstOrDefault(x => x.Channel == channel) ?? ChannelOptions[0];
        IsWeGameChannelSelected = SelectedChannelOption.Channel == WavesChannel.WeGame;

        var launchMode = await GameContext.GameLocalConfig.GetConfigAsync(WavesLaunchModeKey);
        if (!Enum.TryParse<WavesLaunchMode>(launchMode, out var mode))
        {
            mode = WavesLaunchMode.WeGame;
        }

        SelectedLaunchModeOption =
            LaunchModeOptions.FirstOrDefault(x => x.Mode == mode) ?? LaunchModeOptions[0];
        await SelectDefaultLaunchModeAsync(SelectedChannelOption.Channel);
        await RefreshChannelStatusAsync();
    }

    private async Task<string> GetGameFolderAsync()
    {
        return await GameContext.GameLocalConfig.GetConfigAsync(GameLocalSettingName.GameLauncherBassFolder) ?? "";
    }

    private async Task RefreshChannelStatusAsync()
    {
        if (GameContext == null || SelectedChannelOption == null)
        {
            ChannelStatusText = "渠道状态未检测";
            return;
        }

        var folder = await GetGameFolderAsync();
        var status = await _wavesChannelService.GetStatusAsync(
            folder,
            SelectedChannelOption.Channel,
            this.CTS?.Token ?? default
        );
        ChannelStatusText = status.Message;
        ChannelStatusShortText = BuildChannelStatusShortText(status);
    }

    private async Task SelectDefaultLaunchModeAsync(WavesChannel channel)
    {
        var mode = channel == WavesChannel.WeGame ? WavesLaunchMode.WeGame : WavesLaunchMode.Haiyu;
        var option = LaunchModeOptions.FirstOrDefault(x => x.Mode == mode) ?? LaunchModeOptions[0];
        if (SelectedLaunchModeOption != option)
        {
            SelectedLaunchModeOption = option;
        }

        await GameContext.GameLocalConfig.SaveConfigAsync(WavesLaunchModeKey, mode.ToString());
    }

    private static string BuildChannelStatusShortText(WavesChannelStatus status)
    {
        var currentText = GetChannelDisplayName(status.CurrentChannel);
        var backupText = status.SelectedBackupExists ? "备份完整" : "备份缺失";
        var activeText = status.ActivePackageComplete ? "" : " · 文件缺失";
        return $"当前：{currentText} · {backupText}{activeText}";
    }

    private static string GetChannelDisplayName(WavesChannel channel)
    {
        return channel switch
        {
            WavesChannel.Official => "官方",
            WavesChannel.Bilibili => "B服",
            WavesChannel.WeGame => "WeGame",
            _ => "未知",
        };
    }

    private async Task RunChannelOperationAsync(Func<Task> operation, string successMessage)
    {
        if (IsChannelOperationRunning)
        {
            return;
        }

        try
        {
            IsChannelOperationRunning = true;
            await operation();
            await RefreshChannelStatusAsync();
            await TipShow.ShowMessageAsync(successMessage, Symbol.Accept);
        }
        catch (Exception ex)
        {
            await RefreshChannelStatusAsync();
            await TipShow.ShowMessageAsync(ex.Message, Symbol.Clear);
        }
        finally
        {
            IsChannelOperationRunning = false;
        }
    }

    [RelayCommand]
    private async Task RefreshChannelStatus()
    {
        await RefreshChannelStatusAsync();
    }

    [RelayCommand]
    private async Task BackupCurrentChannel()
    {
        await RunChannelOperationAsync(
            async () =>
            {
                var folder = await GetGameFolderAsync();
                var detected = await _wavesChannelService.DetectChannelAsync(folder, this.CTS?.Token ?? default);
                var channel = detected == WavesChannel.Unknown
                    ? SelectedChannelOption.Channel
                    : detected;
                await _wavesChannelService.BackupAsync(folder, channel, true, this.CTS?.Token ?? default);
            },
            "当前渠道备份已刷新"
        );
    }

    [RelayCommand]
    private async Task RefreshSelectedChannelBackup()
    {
        await RunChannelOperationAsync(
            async () =>
            {
                var folder = await GetGameFolderAsync();
                var detected = await _wavesChannelService.DetectChannelAsync(folder, this.CTS?.Token ?? default);
                if (detected != WavesChannel.Unknown && detected != SelectedChannelOption.Channel)
                {
                    throw new InvalidOperationException("当前活动渠道与所选渠道不一致，请先切到正确渠道后再刷新备份。");
                }

                await _wavesChannelService.BackupAsync(
                    folder,
                    SelectedChannelOption.Channel,
                    true,
                    this.CTS?.Token ?? default
                );
            },
            "目标渠道备份已刷新"
        );
    }

    [RelayCommand]
    private async Task SwitchSelectedChannel()
    {
        await RunChannelOperationAsync(
            async () => await RestoreSelectedChannelAsync(),
            "渠道切换完成"
        );
    }

    [RelayCommand]
    private async Task SelectGameProgram()
    {
        var file = await _pickersService.GetFileOpenPicker([".exe"]);
        if (file == null)
        {
            return;
        }

        if (Path.GetFileName(file.Path) != GameContext.Config.GameExeName)
        {
            await TipShow.ShowMessageAsync($"请选择 {GameContext.Config.GameExeName}", Symbol.Clear);
            return;
        }

        var folder = Path.GetDirectoryName(file.Path);
        if (string.IsNullOrWhiteSpace(folder))
        {
            await TipShow.ShowMessageAsync("无法识别游戏目录", Symbol.Clear);
            return;
        }

        await GameContext.GameLocalConfig.SaveConfigAsync(GameLocalSettingName.GameLauncherBassFolder, folder);
        await GameContext.GameLocalConfig.SaveConfigAsync(GameLocalSettingName.GameLauncherBassProgram, file.Path);
        await GameContext.GameLocalConfig.SaveConfigAsync(GameLocalSettingName.LocalGameUpdateing, "False");
        await RefreshChannelStatusAsync();
        GameContext.GameEventPublisher.Publish(new GameContextOutputArgs { Type = GameContextActionType.None });
        await TipShow.ShowMessageAsync("游戏目录已更新", Symbol.Accept);
    }

    [RelayCommand]
    private void OpenChannelBackupFolder()
    {
        var folder = _wavesChannelService.GetDefaultBackupRoot();
        Directory.CreateDirectory(folder);
        Process.Start(
            new ProcessStartInfo(folder)
            {
                UseShellExecute = true,
            }
        );
    }

    private async Task RestoreSelectedChannelAsync()
    {
        if (SelectedChannelOption == null)
        {
            throw new InvalidOperationException("请选择目标渠道。");
        }

        var folder = await GetGameFolderAsync();
        await _wavesChannelService.RestoreAsync(
            folder,
            SelectedChannelOption.Channel,
            this.CTS?.Token ?? default
        );
    }

    protected override async Task<bool> PrepareStartGameAsync()
    {
        if (SelectedChannelOption == null)
        {
            return true;
        }

        if (
            SelectedChannelOption.Channel == WavesChannel.WeGame
            && SelectedLaunchModeOption?.Mode == WavesLaunchMode.Haiyu
        )
        {
            await TipShow.ShowMessageAsync(
                "WeGame 渠道直启会缺少 Rail 启动参数，请使用“通过 WeGame”。",
                Symbol.Clear
            );
            return false;
        }

        try
        {
            await RestoreSelectedChannelAsync();
            await RefreshChannelStatusAsync();
            return true;
        }
        catch (Exception ex)
        {
            await TipShow.ShowMessageAsync(ex.Message, Symbol.Clear);
            return false;
        }
    }

    protected override Task<bool?> TryStartGameOverrideAsync()
    {
        if (
            SelectedChannelOption?.Channel != WavesChannel.WeGame
            || SelectedLaunchModeOption?.Mode != WavesLaunchMode.WeGame
        )
        {
            return Task.FromResult<bool?>(null);
        }

        var startInfo = new ProcessStartInfo(WeGameWutheringWavesProtocol)
        {
            UseShellExecute = true,
        };

        Process.Start(startInfo);
        return Task.FromResult<bool?>(true);
    }

    [ObservableProperty]
    public partial ObservableCollection<Content> Contents { get; set; } = new();

    #region 本地账户卡片

    /// <summary>
    /// 是否正在刷新本地账户状态
    /// </summary>
    [ObservableProperty]
    public partial bool IsLocalUserRefresh { get; set; }

    /// <summary>
    /// 本地账户标题信息
    /// </summary>
    [ObservableProperty]
    public partial string LocalUserTitle { get; set; }

    [ObservableProperty]
    public partial Base Base { get; private set; }



    [ObservableProperty]
    public partial MusicData MusicData { get; private set; }

    [ObservableProperty]
    public partial BattlePass BattlePass { get; private set; }

    [ObservableProperty]
    public partial MotorData MotorData { get; private set; }

    #endregion

    public async override Task ShowCardAsync(bool showCard)
    {
        if (showCard)
        {
            var starter = await this.GameContext.GetLauncherStarterAsync(this.CTS.Token);
            if (starter == null)
                return;
           
            this.SlideShows = starter.Slideshow.ToObservableCollection();
            this.Notice = starter.Guidance.Notice;
            this.News = starter.Guidance.News;
            this.Activity = starter.Guidance.Activity;
            PlayerCardVisibility = Visibility.Visible;
            this.SelectTab = null;
            this.SelectTab = Tabs[0];
            await RefreshLocalGameUser();
        }
        else
        {

            this.Base = null;
            this.MusicData = null;
            this.BattlePass = null;
            this.MotorData = null;
            this.SelectTab = null;
            PlayerCardVisibility = Visibility.Collapsed;
        }
    }



    [RelayCommand]
    private async Task RefreshLocalGameUser(KRSDKLauncherCacheWrapper wrapper = null)
    {
        IsLocalUserRefresh = true;
        var lastSelect = await this.GameContext.GameLocalConfig.GetConfigAsync(
            GameLocalSettingName.LasterSelectLocalUser,
            this.CTS.Token
        );
        if (lastSelect == null)
        {
            LocalUserTitle = "请在右侧选择本地游戏账号信息";
            IsLocalUserRefresh = false;
            SwatchIndex = 2;
            return;
        }
        KRSDKLauncherCacheWrapper? selectItem = null;
        if (wrapper != null)
        {
            selectItem = wrapper;
        }
        else
        {
            var localUsers = await this.GameContext.GetLocalGameOAuthAsync(this.CTS.Token);
            if (localUsers == null || localUsers.Count == 0)
            {
                LocalUserTitle = "未获取到本地游戏账号信息";
                this.Base = null;
                this.MusicData = null;
                this.BattlePass = null;
                this.MotorData = null;
                return;
            }
            foreach (var item in localUsers)
            {
                var code = KrKeyHelper.Xor(item.OauthCode, 5);
                var userPlayers = await GameContext.QueryPlayerInfoAsync(code);
                if (userPlayers == null || userPlayers.Code != 0)
                {

                    this.Base = null;
                    this.MusicData = null;
                    this.BattlePass = null;
                    this.MotorData = null;
                    this.GameContext.SystemEventPublisher.Publish(new()
                    {
                        Message = $"游戏账号:{item.Username}失效",
                        Delay = 5
                    });
                    IsLocalUserRefresh = false;
                    continue;
                }
                foreach (var player in userPlayers.Items)
                {
                    if (player is not WavesQueryPlayerItem wavesPlayer)
                    {
                        continue;
                    }
                    KRSDKLauncherCacheWrapper info = new KRSDKLauncherCacheWrapper(item, wavesPlayer);
                    if (info.GetKey == lastSelect)
                    {
                        selectItem = info;
                        break;
                    }
                }
            }

        }
        if (selectItem == null)
        {
            LocalUserTitle = "未获取到上次选择的本地游戏账号信息";
            this.Base = null;
            this.MusicData = null;
            this.BattlePass = null;
            this.MotorData = null;
            IsLocalUserRefresh = false;

            return;
        }
        if (selectItem.PlayerItem is not WavesQueryPlayerItem playerItem)
        {
            IsLocalUserRefresh = false;
            return;
        }
        LocalUserTitle = playerItem.RoleName;
        var result = await this.GameContext.QueryRoleInfoAsync(
            KrKeyHelper.Xor(selectItem.Cache.OauthCode, 5),
            playerItem.Id,
            playerItem.ServerName
        );
        if (result == null || result.Items == null || result.Items.Count == 0)
        {
            LocalUserTitle = "获取账号信息失败";
            await TipShow.ShowMessageAsync("请重新进入游戏获取信息", Symbol.Clear);
            this.GameContext.SystemEventPublisher.Publish(new()
            {
                Message = $"体力卡片刷新失败，请重新进入游戏获取",
                Delay = 5
            });
            IsLocalUserRefresh = false;
            this.Base = null;
            this.MusicData = null;
            this.BattlePass = null;
            this.MotorData = null;
            return;
        }
        var wavesData = (result.Items[0] as WavesLocalGameRoleItem)!;
        this.Base = wavesData.Base??new();
        this.MusicData = wavesData.MusicData;
        this.BattlePass = wavesData.BattlePass??new();
        this.MotorData = wavesData.MotorData??new();
        SwatchIndex = 1;
        IsLocalUserRefresh = false;
    }

    [RelayCommand]
    public void CopyGameItemId()
    {
        if (Base.Id == null)
            return;
        var package = new DataPackage();
        package.SetText(Base.Id.ToString());
        Clipboard.SetContent(package);
    }
}
