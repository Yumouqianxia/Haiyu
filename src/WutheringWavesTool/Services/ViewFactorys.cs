using Haiyu.Pages.Communitys;
using Waves.Api.Models.CloudGame;
using Waves.Core.Models.CloudGame;

namespace Haiyu.Services;

public class ViewFactorys : IViewFactorys
{
    public ViewFactorys(IAppContext<App> appContext)
    {
        AppContext = appContext;
    }

    public IAppContext<App> AppContext { get; }

    public GetGeetWindow CreateGeetWindow(GeetType type)
    {
        var windw = new GetGeetWindow(WindowNative.GetWindowHandle(AppContext.App.MainWindow), type);
        windw.Manager.MaxHeight = 510;
        windw.Manager.MaxWidth = 700;
        return windw;
    }

    public WindowModelBase ShowSignWindow(GameRoilDataItem role) =>
        this.ShowWindowBase<GamerSignPage>(role);

    public WindowModelBase ShowWindowBase<T>(object data)
        where T : UIElement, IWindowPage
    {
        var win = new WindowModelBase(WindowNative.GetWindowHandle(AppContext.App.MainWindow));
        var page = Instance.Host.Services!.GetRequiredService<T>();
        if (data != null)
            page.SetData(data);
        page.SetWindow(win);
        win.Content = page;
        return win;
    }

    public WindowModelBase ShowAdminDevice()
    {
        var win = new WindowModelBase(WindowNative.GetWindowHandle(AppContext.App.MainWindow));
        var page = Instance.Host.Services!.GetRequiredService<DeviceInfoPage>();
        page.SetWindow(win);
        win.Content = page;
        win.Manager.MaxHeight = 530;
        win.Manager.MaxWidth = 750;
        return win;
    }



    public TransparentWindow CreateTransperentWindow()
    {
        return new TransparentWindow();
    }



    public Window CreateAllowTransparent()
    {
        return new Window();
    }

    public WindowModelBase ShowAnalysisRecordV2(CloudGameLoginSession selectLogin)
    {
        return this.ShowWindowBase<WavesAnalysisRecordPage>(selectLogin);
    }
}
