using Haiyu.ViewModel.ToolkitsViewModel;

namespace Haiyu.Pages.Toolkits;

public sealed partial class AutoKuroTokenPage : Page, IWindowPage
{
    public AutoKuroTokenPage()
    {
        InitializeComponent();
        this.ViewModel = Instance.Host.Services.GetRequiredService<AutoKuroTokenViewModel>();
    }

    public AutoKuroTokenViewModel ViewModel { get; private set; }

    public void Dispose() { }

    public void SetData(object value) { }

    public void SetWindow(Window window)
    {
        this.ViewModel.Window = window;
        this.ViewModel.Window.ExtendsContentIntoTitleBar = true;
        this.titleBar.Window = window;
        this.ViewModel.Window.AppWindow.Closing += AppWindow_Closing;
        this.ViewModel.Window.ApplyWindowsOption(
            new()
            {
                Height = 580,
                Width = 1000,
                MaxHeight = 580,
                MaxWidth =1000,
                MinHeight = 580,
                MinWidth=1000,
                IsMaximizable = false,
                IsMinimizable = false,
                IsResizable = false,
            }
        );
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        this.ViewModel.Window.AppWindow.Closing -= AppWindow_Closing;
        this.ViewModel.Dispose();
        this.ViewModel = null;
    }
}
