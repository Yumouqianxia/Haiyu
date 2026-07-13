using System;
using System.Collections.Generic;
using System.Text;

namespace Haiyu.ViewModel;

public sealed partial class ToolkitViewModel:ViewModelBase
{
    public ToolkitViewModel(IViewFactorys viewFactorys)
    {
        ViewFactorys = viewFactorys;
    }

    public IViewFactorys ViewFactorys { get; }

    [RelayCommand]
    void ShowAutoKuroToken()
    {
        var window = ViewFactorys.ShowAutoKruoTokenWindow();
        window.AppWindow.Show();
    }
}
