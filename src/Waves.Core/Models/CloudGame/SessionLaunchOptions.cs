using System;
using System.Collections.Generic;
using System.Text;
using Waves.Api.Models.CloudGame;
using Waves.Core.Models.CloudGame;

namespace Waves.Core.Models.CloudGame
{
    public class SessionLaunchOptions
    {
        /// <summary>
        /// 云游戏主站页面地址。
        /// </summary>
        public required string GameUrl { get; init; }

        /// <summary>
        /// 登录域预热地址，用于先写入 usercenter 会话。
        /// </summary>
        public string BootstrapUrl { get; init; } = string.Empty;

        /// <summary>
        /// 当前会话使用的 access token。
        /// </summary>
        public string AccessToken { get; init; } = string.Empty;

        /// <summary>
        /// 当前会话使用的 refresh 或 phone token。
        /// </summary>
        public string RefreshToken { get; init; } = string.Empty;

        /// <summary>
        /// 写入 WebView2 Cookie 时使用的域名。
        /// </summary>
        public string CookieDomain { get; init; } = string.Empty;

        /// <summary>
        /// 需要按域名注入到请求中的额外请求头。
        /// </summary>
        public IReadOnlyDictionary<string, string> AdditionalHeaders { get; init; } = new Dictionary<string, string>();

        /// <summary>
        /// 启动前需要写入浏览器环境的 Cookie 集合。
        /// </summary>
        public IReadOnlyDictionary<string, string> Cookies { get; init; } = new Dictionary<string, string>();

        /// <summary>
        /// 启动前需要写入 localStorage 或 sessionStorage 的键值集合。
        /// </summary>
        public IReadOnlyDictionary<string, string> StorageItems { get; init; } = new Dictionary<string, string>();

        /// <summary>
        /// 额外请求头允许注入的主机匹配规则。
        /// </summary>
        public IReadOnlyList<string> HeaderHostPatterns { get; init; } = new List<string>();


        /// <summary>
        /// 本次启动使用的串流画质配置。
        /// </summary>
        public StreamQualityOptions Quality { get; init; } = StreamQualityOptions.Default;

        /// <summary>
        /// 当前系统 DPI 值（如 96、120、144、168 等），对齐官方 getDeviceDPI()。
        /// 桥页 Welink SDK 初始化时需要以此为准，而非硬编码 120。
        /// </summary>
        public int StreamDpi { get; init; } = 120;

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
    string Preset = "clear")
{
    public const string SmoothPreset = "smooth";

    public const string ClearPreset = "clear";

    public static readonly StreamQualityOptions Default = FromOfficialPreset(ClearPreset, 60, true);

    public string ResolutionKey => $"{Width}x{Height}";

    public int BitRateMax => BitRate;

    public string OfficialPreset => Preset;

    public string OfficialPresetLabel => string.Equals(OfficialPreset, SmoothPreset, StringComparison.OrdinalIgnoreCase) ? "流畅" : "清晰";

    public string ImageEnhancementLabel => EnableImageEnhancement ? "开启" : "关闭";

    public static StreamQualityOptions FromOfficialPreset(string? preset, int fps, bool enableImageEnhancement = false)
    {
        var normalizedFps = fps == 30 ? 30 : 60;
        var effectivePreset = string.Equals(preset, SmoothPreset, StringComparison.OrdinalIgnoreCase) ? SmoothPreset : ClearPreset;
        return string.Equals(effectivePreset, ClearPreset, StringComparison.OrdinalIgnoreCase)
            ? new StreamQualityOptions(18000, 8000, normalizedFps, 1920, 1080, 21, "0", enableImageEnhancement, ClearPreset)
            : new StreamQualityOptions(5000, 2500, normalizedFps, 1280, 720, 21, "0", enableImageEnhancement, SmoothPreset);
    }
}


