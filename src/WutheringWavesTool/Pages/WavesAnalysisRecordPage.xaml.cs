using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Windows.Foundation;
using Waves.Core.Models.CloudGame;

namespace Haiyu.Pages;

/// <summary>
/// 新抽卡页面，自动合并，自动分析并计算结果
/// </summary>
public sealed partial class WavesAnalysisRecordPage : Page,IWindowPage
{
    public WavesAnalysisRecordViewModel ViewModel { get; private set; }

    public WavesAnalysisRecordPage()
    {
        InitializeComponent();
        this.ViewModel = Instance.Host.Services.GetRequiredService<WavesAnalysisRecordViewModel>();
    }

    public void SetWindow(Window window)
    {
        this.ViewModel.Initialization(window);
        this.ViewModel.Window.Closed += Window_Closed;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        this.Dispose();
    }

    public void SetData(object value)
    {
        if(value is CloudGameLoginSession session)
        {
            this.ViewModel.Session = session;
        }
    }

    public void Dispose()
    {
        this.ViewModel.Dispose();
        this.ViewModel = null;
    }
}
