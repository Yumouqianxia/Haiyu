using Haiyu.ServiceHost;

namespace Haiyu.Pages.Dialogs;

public sealed partial class GameEnhancedDialog : ContentDialog,IDialog
{
    public GameEnhancedDialog()
    {
        InitializeComponent();
        this.ViewModel = Instance.Host.Services.GetRequiredService<GameEnhancedViewModel>();
        this.RequestedTheme = Instance.Host.Services.GetRequiredService<IThemeService>().CurrentTheme;
    }

    public GameEnhancedViewModel ViewModel { get; }

    public void SetData(object data)
    {
        this.xboxEnable.IsChecked = ViewModel.XboxConfig.IsEnable;
    }

    private void xboxEnable_Checked(object sender, RoutedEventArgs e)
    {
        ViewModel.XboxConfig.IsEnable = true;
    }

    private void xboxEnable_Unchecked(object sender, RoutedEventArgs e)
    {
        ViewModel.XboxConfig.IsEnable = false;
    }
}
