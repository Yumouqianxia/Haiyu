namespace Haiyu.Pages;

public sealed partial class WavesAnalysisRecordPage : Page,IWindowPage
{
    public WavesAnalysisRecordViewModel ViewModel { get; private set; }

    public WavesAnalysisRecordPage()
    {
        InitializeComponent();
        this.ViewModel = Instance.Host.Services.GetRequiredService<WavesAnalysisRecordViewModel>();
        this.RequestedTheme = Instance.Host.Services.GetRequiredService<IThemeService>().CurrentTheme;
    }

    public void SetWindow(Window window)
    {
        this.ViewModel.Initialization(window);
        //window.AppWindow.TitleBar.PreferredTheme = this.RequestedTheme = Instance.Host.Services.GetRequiredService<IThemeService>().CurrentTheme;
        this.titleBar.Window = window;
        
        this.ViewModel.Window.AppWindow.Closing += AppWindow_Closing;
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        this.Dispose();
    }

    public void SetData(object value)
    {
        if(value is CloudGameLoginSession session)
        {
            this.ViewModel.SetSessionAsync(session);
        }
    }

    public void Dispose()
    {
        this.Bindings.StopTracking();
        this.ViewModel.Dispose();
        this.ViewModel = null;
        GC.Collect();
    }
}
