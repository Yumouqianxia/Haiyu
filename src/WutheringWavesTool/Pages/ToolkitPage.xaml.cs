namespace Haiyu.Pages;

public sealed partial class ToolkitPage : Page,IPage
{
    public ToolkitPage()
    {
        InitializeComponent();
        this.ViewModel = Instance.Host.Services.GetRequiredService<ToolkitViewModel>();
    }

    public Type PageType => typeof(ToolkitPage);

    public ToolkitViewModel ViewModel { get; }
}
