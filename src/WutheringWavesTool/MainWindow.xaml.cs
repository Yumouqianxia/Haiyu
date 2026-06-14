namespace Haiyu;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "Haiyu";
        this.AppWindow.SetIcon(AppDomain.CurrentDomain.BaseDirectory + "Assets/appLogo.ico");
        this.IsResizable = false;
        NativeWindowHelper.ForceDisableMaximize(this, targetDipWidth: 1150, targetDipHeight: 650);
        this.SystemBackdrop = new MicaBackdrop();
    }


}
