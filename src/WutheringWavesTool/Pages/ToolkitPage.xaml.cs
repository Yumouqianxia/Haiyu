namespace Haiyu.Pages;

public sealed partial class ToolkitPage : Page,IPage
{
    public ToolkitPage()
    {
        InitializeComponent();
    }

    public Type PageType => typeof(ToolkitPage);
}
