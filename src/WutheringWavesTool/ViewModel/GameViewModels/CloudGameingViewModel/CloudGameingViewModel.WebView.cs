using System;
using System.Collections.Generic;
using System.Text;

namespace Haiyu.ViewModel.GameViewModels
{
    partial class CloudGameingViewModel
    {
        public async Task<bool> RequestExitAsync()
        {
            return await InvokeStreamControlAsync("requestExit");
        }

        public async Task<bool> SetVolumeAsync(int percent)
        {
            return await InvokeStreamControlAsync("setVolume", percent);
        }

        public async Task<bool> SetMutedAsync(bool muted)
        {
            return await InvokeStreamControlAsync("setMuted", muted);
        }

        public async Task<bool> SetImageEnhancementAsync(bool enabled)
        {
            return await InvokeStreamControlAsync("setImageEnhancement", enabled);
        }

        public async Task<bool> ApplyQualityProfileAsync(StreamQualityOptions quality)
        {
            return await InvokeStreamControlAsync(
                "applyQualityProfile",
                new
                {
                    bitRate = quality.BitRate,
                    bitRateMin = quality.BitRateMin,
                    bitRateMax = quality.BitRateMax,
                    fps = quality.Fps,
                    width = quality.Width,
                    height = quality.Height,
                    codecType = quality.CodecType,
                    streamStrategy = quality.StreamStrategy,
                    enableImageEnhancement = quality.EnableImageEnhancement,
                    dpi = quality.DPI
                }
            );
        }

        

        private async Task<bool> InvokeStreamControlAsync(string methodName, params object[] args)
        {
            if (WebView2?.CoreWebView2 is null)
            {
                return false;
            }

            var script = methodName switch
            {
                "requestExit" => "window.__KURO_STREAM_CONTROL__?.requestExit?.()",
                "setVolume" => $"window.__KURO_STREAM_CONTROL__?.setVolume?.({JsonSerializer.Serialize(args.ElementAtOrDefault(0))})",
                "setMuted" => $"window.__KURO_STREAM_CONTROL__?.setMuted?.({JsonSerializer.Serialize(args.ElementAtOrDefault(0))})",
                "setImageEnhancement" => $"window.__KURO_STREAM_CONTROL__?.setImageEnhancement?.({JsonSerializer.Serialize(args.ElementAtOrDefault(0))})",
                "applyQualityProfile" => $"window.__KURO_STREAM_CONTROL__?.applyQualityProfile?.({JsonSerializer.Serialize(args.ElementAtOrDefault(0))})",
                _ => throw new NotSupportedException($"不支持的串流控制方法: {methodName}")
            };

            try
            {
                await WebView2.CoreWebView2.ExecuteScriptAsync(script);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
