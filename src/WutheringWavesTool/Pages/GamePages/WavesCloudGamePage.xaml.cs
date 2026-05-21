using Haiyu.ViewModel.GameViewModels;

namespace Haiyu.Pages.GamePages;

public sealed partial class WavesCloudGamePage : Page,IPage
{
    public WavesCloudGamePage()
    {
        InitializeComponent();
        this.ViewModel = Instance.Host.Services.GetRequiredService<WavesCloudGameViewModel>();
    }

    public Type PageType => typeof(WavesCloudGamePage);


    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        this.Bindings.StopTracking();
        this.ViewModel.Dispose();
        this.ViewModel = null;
        GC.Collect();
        base.OnNavigatedFrom(e);
    }

    public WavesCloudGameViewModel ViewModel { get; set; }
}
