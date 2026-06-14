using Waves.Core.Models.Enums;

namespace Haiyu.ViewModel;

public sealed partial class KuroGameSettingViewModel:DialogViewModelBase
{
    public IGameContextV2 GameContext { get; private set; }

    public GameType GameType { get; private set; }

    [RelayCommand]
    async Task Loaded()
    {
        var speed = await this.GameContext.GameLocalConfig.GetConfigAsync(GameLocalSettingName.LimitSpeed);
        if(long.TryParse(speed,out var speedValue))
        {
            this.DownloadSpeedLimit = speedValue;
        }
    }

    public void SetConfig(GameSettingDialogConfig config)
    {
        this.GameContext = Instance.Host.Services.GetRequiredKeyedService<IGameContextV2>(config.CoreName);
        this.GameType = this.GameContext.GameType;
    }

    [ObservableProperty]
    public partial double DownloadSpeedLimit { get; set; }

    

    public async override Task BeforeCloseAsync() 
    {
        await this.GameContext.SetDownloadSpeedAsync((long) DownloadSpeedLimit);
        await base.BeforeCloseAsync();
    }
}
