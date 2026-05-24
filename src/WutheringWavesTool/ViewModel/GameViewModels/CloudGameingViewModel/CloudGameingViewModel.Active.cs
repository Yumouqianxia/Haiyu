using System;
using System.Collections.Generic;
using System.Text;
using Waves.Api.Models.CloudGame;

namespace Haiyu.ViewModel.GameViewModels;

partial class CloudGameingViewModel
{
    [ObservableProperty]
    public partial string DelayTime { get; set; }

    [ObservableProperty]
    public partial string Fps { get; set; }

    [ObservableProperty]
    public partial string Network { get; set; }

    [ObservableProperty]
    public partial string PacketLossRate { get; set; }

    public void UpdateNetworkDisplay(WelinkMessage message)
    {
        this.Window.DispatcherQueue.TryEnqueue(() =>
        {
            this.DelayTime = $"延迟：{message.Detail.NetWorkDelay} ms";
            this.Fps = $"客户端：{message.Detail.Fps}帧";
            this.Network = $"带宽：{message.Detail.Bitrate / 8 / 1024.0:0.#} MB/s";
            this.PacketLossRate = $"丢包率：{message.Detail.PacketLossRate}%";
        });
    }
}
