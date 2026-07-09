namespace Waves.Core.GameContext.KruoGameContextBaseV2.Common;

/// <summary>
/// 执行某些任务结束后写入的数据信息
/// </summary>
public class WriteGameResourceConfig : IAsyncDisposable
{
    private readonly GameLocalConfig GameLocalConfig;
    private readonly GameLauncherSource source;
    private readonly KuroGameApiConfig kuroGameApiConfig;
    private readonly LoggerService logger;

    public WriteGameResourceConfig(GameLocalConfig config, GameLauncherSource launcherSource,KuroGameApiConfig kuroGameApiConfig, Services.LoggerService logger)
    {
        this.GameLocalConfig = config;
        this.source = launcherSource;
        this.kuroGameApiConfig = kuroGameApiConfig;
        this.logger = logger;
    }

    /// <summary>
    /// DownloadAndVerifyResource类写入后执行方法
    /// </summary>
    /// <returns></returns>
    public async Task WriteDownloadComplateAsync(IGameEventPublisher<GameContextOutputArgs> gameEventPublisher,bool isSync = false)
    {
        var currentVersion = await GameLocalConfig.GetConfigAsync(
            GameLocalSettingName.LocalGameVersion
        );
        var installFolder = await GameLocalConfig.GetConfigAsync(
            GameLocalSettingName.GameLauncherBassFolder
        );
        if (string.IsNullOrWhiteSpace(currentVersion))
        {
            await this.GameLocalConfig.SaveConfigAsync(
                GameLocalSettingName.LocalGameVersion,
                source.ResourceDefault.Version
            );
        }
        await this.GameLocalConfig.SaveConfigAsync(
            GameLocalSettingName.LocalGameVersion,
            source.ResourceDefault.Version
        );
        await this.GameLocalConfig.SaveConfigAsync(
            GameLocalSettingName.LocalGameUpdateing,
            "False"
        );

        await this.GameLocalConfig.SaveConfigAsync(
            GameLocalSettingName.GameLauncherBassProgram,
            $"{installFolder}\\{this.kuroGameApiConfig.GameExeName}"
        );
    }

    public async Task WriteDownloadCancelAsync(IGameEventPublisher<GameContextOutputArgs> gameEventPublisher, bool isSync = false)
    {
        var currentVersion = await GameLocalConfig.GetConfigAsync(
            GameLocalSettingName.LocalGameVersion
        );
        var installFolder = await GameLocalConfig.GetConfigAsync(
            GameLocalSettingName.GameLauncherBassFolder
        );
        if (string.IsNullOrWhiteSpace(currentVersion))
        {
            await this.GameLocalConfig.SaveConfigAsync(
                GameLocalSettingName.LocalGameVersion,
            ""
            );
        }
        await this.GameLocalConfig.SaveConfigAsync(
            GameLocalSettingName.LocalGameVersion,
            ""
        );
        await this.GameLocalConfig.SaveConfigAsync(
            GameLocalSettingName.LocalGameUpdateing,
            "False"
        );

        await this.GameLocalConfig.SaveConfigAsync(
            GameLocalSettingName.GameLauncherBassProgram,
            ""
        );
    }
    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task WriteDownloadAndUpDateResultAsync(GameLauncherSource source, InstallOption option)
    {
        if (option.IsAdvance && source.Predownload != null)
        {
            await this.GameLocalConfig.SaveConfigAsync(
                  GameLocalSettingName.LocalGameVersion,
                  source.Predownload.Version
              );
            await this.GameLocalConfig.SaveConfigAsync(GameLocalSettingName.ProdIsAdvance, "True");
            await this.GameLocalConfig.SaveConfigAsync(GameLocalSettingName.ProdDownloadFolderDone, "False");
            await this.GameLocalConfig.SaveConfigAsync(GameLocalSettingName.ProdDownloadPath, "");
            await this.GameLocalConfig.SaveConfigAsync(GameLocalSettingName.ProdDownloadVersion, "");
        }
        else
        {
            await this.GameLocalConfig.SaveConfigAsync(
                GameLocalSettingName.LocalGameVersion,
                source.ResourceDefault.Version
            );
        }
        var installFolder = await GameLocalConfig.GetConfigAsync(
            GameLocalSettingName.GameLauncherBassFolder
        );
        await this.GameLocalConfig.SaveConfigAsync(
            GameLocalSettingName.LocalGameUpdateing,
            "False"
        );

        await this.GameLocalConfig.SaveConfigAsync(
            GameLocalSettingName.GameLauncherBassProgram,
            $"{installFolder}\\{kuroGameApiConfig.GameExeName}"
        );
    }
}
