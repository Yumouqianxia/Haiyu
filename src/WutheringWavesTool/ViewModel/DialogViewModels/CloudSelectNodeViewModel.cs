using Waves.Api.Models.CloudGame;
using Waves.Core.Contracts.CloudGame;
using Waves.Core.Services.CloudGameServices;

namespace Haiyu.ViewModel.DialogViewModels;

public sealed partial class CloudSelectNodeViewModel:DialogViewModelBase
{
    public IWavesCloudGameService KuroCloudGameContext { get; }
    public WavesCloudSurvivalService WavesCloudSurvivalService { get; }

    public CloudSelectNodeViewModel(IWavesCloudGameService kuroCloudGameContext,WavesCloudSurvivalService wavesCloudSurvivalService)
    {
        this.KuroCloudGameContext = kuroCloudGameContext;
        WavesCloudSurvivalService = wavesCloudSurvivalService;
    }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<CloudGameNode> Nodes { get; set; }


    [ObservableProperty]
    public partial CloudGameNode? SelectNode { get; set; }


    public string Id { get;  set; }

    [RelayCommand]
    private async Task RefreshNodesAsync()
    {
        IsRefreshing = true;
        var session = WavesCloudSurvivalService.Cache.TryGet(Id);
        if(session == null)
        {
            SelectNode = null;
            this.Close();
            this.Dispose();
            return;
        }
        var nodes = await KuroCloudGameContext.GetPingGameNodeAsync(session,this.CTS.Token);
        this.Nodes = new(nodes.Data);
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task Invoke()
    {
        this.Close();
    }

    [RelayCommand]
    private void CloseDialog()
    {
        this.SelectNode = null;
        this.Close();
    }
}
