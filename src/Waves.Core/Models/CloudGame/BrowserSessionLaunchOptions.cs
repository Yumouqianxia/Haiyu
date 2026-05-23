using System;
using System.Collections.Generic;
using System.Text;
using Waves.Api.Models.CloudGame;
using Waves.Core.Models.CloudGame;
using Waves.Core.Models.Enums;

namespace Waves.Core.Models.CloudGame
{
    public class BrowserSessionLaunchOptions
    {

        public string BootstrapUrl { get; init; } = string.Empty;

        public string AccessToken { get; init; } = string.Empty;

        public string RefreshToken { get; init; } = string.Empty;

        public string CookieDomain { get; init; } = string.Empty;

        public IReadOnlyDictionary<string, string> AdditionalHeaders { get; init; } =
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> Cookies { get; init; } =
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> StorageItems { get; init; } =
            new Dictionary<string, string>();

        public IReadOnlyList<string> HeaderHostPatterns { get; init; } = new List<string>();

        public StreamQualityOptions Quality { get; init; } = StreamQualityOptions.Default;

        public int StreamDpi { get; init; } = 120;

        public CloudGameStreamSession StreamOptions { get; internal set; }

        public string DispatchMessage { get; set; }

        public bool IsComplete { get; internal set; }

        internal BrowserSessionLaunchOptions Clone()
        {
            return new BrowserSessionLaunchOptions()
            {
                AccessToken = this.AccessToken,
                Quality = this.Quality,
                AdditionalHeaders = this.AdditionalHeaders,
                BootstrapUrl = this.BootstrapUrl,
                CookieDomain = this.CookieDomain,
                Cookies = this.Cookies,
                HeaderHostPatterns = this.HeaderHostPatterns,
                RefreshToken = this.RefreshToken,
                StorageItems = this.StorageItems,
                StreamDpi = this.StreamDpi,
                StreamOptions = this.StreamOptions,
            };
        }
    }
}

public sealed record StreamQualityOptions(
    int BitRate,
    int BitRateMin,
    int Fps,
    int Width,
    int Height,
    int CodecType,
    string StreamStrategy,
    bool EnableImageEnhancement,
    int DPI,
    CloudQualityType Type = CloudQualityType.Native
)
{
    public const string SmoothPreset = "smooth";

    public const string ClearPreset = "clear";

    public static readonly StreamQualityOptions Default = FromOfficialPreset(ClearPreset, 60, true);

    public string ResolutionKey => $"{Width}x{Height}";

    public int BitRateMax => BitRate;

    public CloudQualityType OfficialPreset => Type;

    public static StreamQualityOptions FromOfficialPreset(
        string? preset,
        int fps,
        bool enableImageEnhancement = false
    )
    {
        var normalizedFps = fps == 30 ? 30 : 60;
        var effectivePreset = string.Equals(
            preset,
            SmoothPreset,
            StringComparison.OrdinalIgnoreCase
        )
            ? SmoothPreset
            : ClearPreset;
        return string.Equals(effectivePreset, ClearPreset, StringComparison.OrdinalIgnoreCase)
            ? new StreamQualityOptions(
                18000,
                8000,
                normalizedFps,
                1920,
                1080,
                21,
                "0",
                enableImageEnhancement,
                128,
                CloudQualityType.Clarity
            )
            : new StreamQualityOptions(
                5000,
                2500,
                normalizedFps,
                1280,
                720,
                21,
                "0",
                enableImageEnhancement,
                128,
                CloudQualityType.Native
            );
    }
}
