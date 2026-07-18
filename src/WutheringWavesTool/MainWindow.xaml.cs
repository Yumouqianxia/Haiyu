namespace Haiyu;

public sealed partial class MainWindow : WindowEx
{
    public static WindowsOption DefaultWindowsOption =>
        new()
        {
            Width = 1150,
            Height = 650,
            IsResizable = false,
            IsMaximizable = false,
            CenterOnScreen = true,
        };

    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "Haiyu";
        this.AppWindow.SetIcon(AppDomain.CurrentDomain.BaseDirectory + "Assets/appLogo.ico");
        this.ApplyWindowsOption(DefaultWindowsOption);
        NativeWindowHelper.ForceDisableMaximize(this, targetDipWidth: 1150, targetDipHeight: 650);
        this.SystemBackdrop = new MicaBackdrop();
    }
}
