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

namespace Haiyu.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
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
                this.ViewModel.Session = session;
            }
        }

        public void Dispose()
        {
            this.ViewModel.Dispose();
            this.ViewModel = null;
        }
    }
}
