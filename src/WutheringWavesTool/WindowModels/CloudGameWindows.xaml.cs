using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Waves.Api.Models.CloudGame;
using Waves.Core.Models.CloudGame;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

namespace Haiyu.WindowModels
{
    public sealed partial class CloudGameWindows : Window
    {
        

        private bool _isClosingRequested;
        private bool _cursorHidden;
        private bool _systemCursorSchemeOverridden;
        private DispatcherTimer _cursorTimer;
        private IntPtr _windowHandle;
        private bool _windowMessageSubclassInstalled;
        private bool _cursorHotKeyRegistered;
        private IntPtr _webViewChildHandle;
        private bool _webViewCursorSubclassInstalled;
        private SUBCLASSPROC _windowMessageSubclassProc;
        private SUBCLASSPROC _webViewCursorSubclassProc;

        public CloudGameWindows(BrowserSessionLaunchOptions option)
        {
            InitializeComponent();
            this.AppWindow.Closing += CloudGameWindows_Closing;
            this.Closed += CloudGameWindows_Closed;

            Option = option;
        }

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

        private async void CloudGameWindows_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if (_isClosingRequested)
            {
                return;
            }

            _isClosingRequested = true;
            args.Cancel = true;

            try
            {
                // 先走桥页内的官方退出串流函数，再允许宿主关闭窗口。
                await RequestExitAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            ShowSystemCursor();
            Close();
        }

        private void CloudGameWindows_Closed(object sender, WindowEventArgs args)
        {
            RemoveWindowMessageSubclass();
            RemoveWebViewCursorSubclass();
            ShowSystemCursor();
        }

        public BrowserSessionLaunchOptions Option { get; }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            _browser!.NavigationStarting += Browser_NavigationStarting;
            _browser.NavigationCompleted += Browser_NavigationCompleted;

            await _browser.EnsureCoreWebView2Async();
            _browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _browser.CoreWebView2.Settings.AreDevToolsEnabled = true;
            _browser.CoreWebView2.Settings.IsPinchZoomEnabled = false;
            _browser.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
            _browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _browser.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            EnsureWindowMessageSubclass();

            await ApplyLaunchOptionsAsync();
            _browser.CoreWebView2.Navigate("https://kuro-stream.local/bridge.html");
            _browser.CoreWebView2.OpenDevToolsWindow();
        }

        private void CoreWebView2_WebMessageReceived(
            CoreWebView2 sender,
            CoreWebView2WebMessageReceivedEventArgs args
        )
        {
            try
            {
                using var document = JsonDocument.Parse(args.WebMessageAsJson);
                var root = document.RootElement;

                if (!root.TryGetProperty("type", out var typeElement))
                {
                    return;
                }

                var messageType = typeElement.GetString();
                var message = root.TryGetProperty("message", out var messageElement)
                    ? messageElement.GetString()
                    : string.Empty;

                switch (messageType)
                {
                    case "status":
                        Debug.WriteLine(message);
                        break;
                    case "warning":
                        Debug.WriteLine(message);
                        break;
                    case "launch-dispatched":
                        Debug.WriteLine(message);
                        break;
                    case "first-frame":
                        Debug.WriteLine(message);
                        break;
                    case "quality-change":
                        Debug.WriteLine(message);
                        break;
                    case "network-stat":
                        Debug.WriteLine(message);
                        break;
                    case "cursor-data":
                        Debug.WriteLine(args.WebMessageAsJson);
                        break;
                    case "pointer-lock":
                        Debug.WriteLine(args.WebMessageAsJson);
                        HandlePointerLockMessage(root);
                        break;
                    case "error":
                        Debug.WriteLine(message);
                        ShowSystemCursor();
                        break;
                }
            }
            catch (Exception ex) { }
        }

        private void Browser_NavigationCompleted(
            WebView2 sender,
            CoreWebView2NavigationCompletedEventArgs args
        )
        {
            TryInstallWebViewCursorSubclass();

            if (!args.IsSuccess)
            {
                ShowSystemCursor();
            }
        }

        private void Browser_NavigationStarting(
            WebView2 sender,
            CoreWebView2NavigationStartingEventArgs args
        )
        {
            ShowSystemCursor();
        }

        private void HandlePointerLockMessage(JsonElement root)
        {
            //var locked = root.TryGetProperty("locked", out var lockedElement)
            //    && lockedElement.ValueKind is JsonValueKind.True or JsonValueKind.False
            //    && lockedElement.GetBoolean();
            //// locked=true 表示需要隐藏；locked=false 表示明确恢复。
            //if (locked)
            //{
            //    HideSystemCursor();
            //    return;
            //}

            //ShowSystemCursor();
        }

        // Calls the bridge-page helper that wraps the official Welink stream controls.
        private async Task<bool> InvokeStreamControlAsync(string methodName, params object[] args)
        {
            if (_browser?.CoreWebView2 is null)
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
                await _browser.CoreWebView2.ExecuteScriptAsync(script);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task ApplyLaunchOptionsAsync()
        {
            if (_browser?.CoreWebView2 is null)
            {
                return;
            }

            var core = _browser.CoreWebView2;

            var requiresWebResourceInterceptor = false;

            if (Option.StreamOptions is not null)
            {
                core.AddWebResourceRequestedFilter(
                    "https://kuro-stream.local/bridge.html",
                    CoreWebView2WebResourceContext.Document
                );
                requiresWebResourceInterceptor = true;
            }

            if (Option.AdditionalHeaders.Count > 0)
            {
                core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                requiresWebResourceInterceptor = true;
            }

            if (requiresWebResourceInterceptor)
            {
                core.WebResourceRequested += CoreWebView2_WebResourceRequested;
            }

            if (Option.StreamOptions is not null)
            {
                return;
            }

            ApplyCookies(core.CookieManager);

            var bootstrapScript = BuildBootstrapScript();
            if (!string.IsNullOrWhiteSpace(bootstrapScript))
            {
                await core.AddScriptToExecuteOnDocumentCreatedAsync(bootstrapScript);
            }
        }

        private string BuildBootstrapScript()
        {
            if (
                string.IsNullOrWhiteSpace(Option.AccessToken)
                && string.IsNullOrWhiteSpace(Option.RefreshToken)
                && Option.StorageItems.Count == 0
            )
            {
                return string.Empty;
            }

            var payloadJson = JsonSerializer.Serialize(
                new CloudBootstrapScript()
                {
                    AccessToken = Option.AccessToken,
                    RefreshToken = Option.RefreshToken,
                    StorageItems = Option.StorageItems,
                },
                CloudGameContext.Default.CloudBootstrapScript
            );

            var bootstrapScript = $$"""
(() => {
  const payload = {{payloadJson}};
  const writeStore = (store) => {
    if (!store || !payload.storageItems) {
      return;
    }

    for (const [key, value] of Object.entries(payload.storageItems)) {
      try {
        store.setItem(key, value ?? "");
      } catch {
      }
    }
  };

  try {
    window.__KURO_LAUNCH_OPTIONS__ = payload;
  } catch {
  }

  writeStore(window.localStorage);
  writeStore(window.sessionStorage);

  try {
    window.dispatchEvent(new CustomEvent("kuro-launch-bootstrap", { detail: payload }));
  } catch {
  }
})();
""";

            return bootstrapScript;
        }

        private static string BuildKuroSiteInitializationScript(
            IReadOnlyDictionary<string, string> storageItems,
            bool autoLaunchGame
        )
        {
            var storageItemsJson = JsonSerializer.Serialize(
                storageItems,
                CloudGameContext.Default.IReadOnlyDictionaryStringString
            );
            var currentUserSessionKey =
                storageItems.Keys.FirstOrDefault(key =>
                    key.StartsWith(
                        "useMcCloudGameUserSession@Official#",
                        StringComparison.OrdinalIgnoreCase
                    )
                ) ?? string.Empty;
            var autoLaunchLiteral = autoLaunchGame ? "true" : "false";

            return $$"""
(() => {
    const storageItems = {{storageItemsJson}};
    const mainHost = "mc.kurogames.com";
    const loginHost = "usercenter.kurogames.com";
    const sessionKeys = new Set(["McCloudSessionId", "show_user_name"]);
    const officialUserSessionPrefix = "useMcCloudGameUserSession@Official#";
    const currentUserSessionKey = "{{currentUserSessionKey}}";
    const autoLaunchGame = {{autoLaunchLiteral}};

    const write = (store, key, value) => {
        try {
            store?.setItem?.(key, value ?? "");
        } catch {
        }
    };

    const remove = (store, key) => {
        try {
            store?.removeItem?.(key);
        } catch {
        }
    };

    const parseJson = (value) => {
        if (!value) {
            return null;
        }

        try {
            return JSON.parse(value);
        } catch {
            return null;
        }
    };

    const applyWindowState = () => {
        try {
            const userPayload = parseJson(storageItems.G152);
            const sdkLoginInfo = parseJson(storageItems.sdkLoginInfo);
            const krSdk = parseJson(storageItems.__KrSDK__);

            if (userPayload) {
                window.__KURO_USER_JSON__ = userPayload;
            }

            if (sdkLoginInfo) {
                window.__KURO_SDK_LOGIN_INFO__ = sdkLoginInfo;
            }

            if (krSdk) {
                window.__KURO_KR_SDK__ = krSdk;
            }

            window.__KURO_SKIP_AUTO_ENTER_GAME__ = !autoLaunchGame;
        } catch {
        }
    };

    const clearStaleOfficialSessionKeys = () => {
        try {
            const localKeys = Object.keys(window.localStorage ?? {});
            for (const key of localKeys) {
                if (!key.startsWith(officialUserSessionPrefix)) {
                    continue;
                }

                if (key !== currentUserSessionKey) {
                    remove(window.localStorage, key);
                }
            }
        } catch {
        }
    };

    const hydrateOfficialState = () => {
        const host = location.host;
        clearStaleOfficialSessionKeys();
        applyWindowState();

        for (const [key, value] of Object.entries(storageItems)) {
            if (sessionKeys.has(key)) {
                write(window.sessionStorage, key, value);
                if (host === mainHost) {
                    write(window.localStorage, key, value);
                }
                continue;
            }

            if (host === mainHost || host === loginHost) {
                write(window.localStorage, key, value);
            }
        }
    };

    const isStreaming = () => {
        const text = document.body?.innerText || "";
        return text.includes("延迟：") && text.includes("FPS：");
    };

    const tryLaunch = () => {
        if (!autoLaunchGame || location.host !== mainHost || isStreaming()) {
            return;
        }

        const candidates = Array.from(document.querySelectorAll("button, [role='button'], div, span"));
        const launchButton = candidates.find((element) => {
            const text = (element.textContent || "").replace(/\s+/g, "");
            return text.startsWith("进入游戏");
        });

        if (launchButton instanceof HTMLElement) {
            launchButton.click();
        }
    };

    hydrateOfficialState();

    if (location.host === mainHost && autoLaunchGame) {
        const startedAt = Date.now();
        const timer = setInterval(() => {
            hydrateOfficialState();
            tryLaunch();

            if (isStreaming() || Date.now() - startedAt > 45000) {
                clearInterval(timer);
            }
        }, 1000);
    }

    window.addEventListener("load", () => {
        hydrateOfficialState();
        tryLaunch();
    }, { once: true });
})();
""";
        }

        private async void CoreWebView2_WebResourceRequested(
            object? sender,
            CoreWebView2WebResourceRequestedEventArgs e
        )
        {
            if (
                _browser?.CoreWebView2 is not null
                && Option.StreamOptions is not null
                && string.Equals(
                    e.Request.Uri,
                    "https://kuro-stream.local/bridge.html",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                Stream outputStream = randomAccessStream.AsStreamForRead();
                randomAccessStream.Seek(0);
                string html = BuildStreamBridgeHtml(Option);

                var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));
                e.Response = _browser.CoreWebView2.Environment.CreateWebResourceResponse(
                    await stream.ConvertStreamToRandomAccessStream(),
                    200,
                    "OK",
                    "Content-Type: text/html; charset=utf-8"
                );
                return;
            }

            if (!Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out var requestUri))
            {
                return;
            }

            foreach (var pair in Option.AdditionalHeaders)
            {
                try
                {
                    e.Request.Headers.SetHeader(pair.Key, pair.Value);
                }
                catch { }
            }
        }

        private string BuildStreamBridgeHtml(BrowserSessionLaunchOptions option)
        {
            var dispatchMessageJson = option.StreamOptions.DispatchMessage;
            var storageItemsJson = JsonSerializer.Serialize(
                option.StorageItems,
                CloudGameContext.Default.IReadOnlyDictionaryStringString
            );

            var scriptUrlJson = $"\"{option.StreamOptions.ScriptUrl}\"";
            var bridgeConfigJson = JsonSerializer.Serialize(
                new BridgeConfig()
                {
                    Id = "kuro-stream-surface",
                    TenantKey = Option.StreamOptions.TenantKey,
                    IspUrl = "https://paas-sdk.vlinkcloud.cn",
                    VideoPoster = string.Empty,
                    GameId = Option.StreamOptions.GameId,
                    NodeId = string.Empty,
                    EnableClipBoard = true,
                    MouseShortcut = (string?)null,
                    LockPoint = true,
                    EnvType = "pc",
                    FillVideo = false,
                    EnableInitSpeed = false,
                    UseGamePlayLayer = true,
                    EnableReportLog = true,
                    EnableReconnect = true,
                    BitRate = Option.Quality.BitRate,
                    BitRateMin = Option.Quality.BitRateMin,
                    BitRateMax = Option.Quality.BitRateMax,
                    Fps = Option.Quality.Fps,
                    TargetWidth = Option.Quality.Width,
                    TargetHeight = Option.Quality.Height,
                    CodecType = Option.Quality.CodecType,
                    StreamStrategy = Option.Quality.StreamStrategy,
                    EnableImageEnhancement = Option.Quality.EnableImageEnhancement,
                    Dpi = Option.StreamDpi,
                },CloudGameContext.Default.BridgeConfig
            );

            return $$"""
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Kuro Stream Bridge</title>
    <style>
        :root {
            color-scheme: dark;
            --bg: #05080d;
            --panel: rgba(8, 12, 18, 0.9);
            --border: rgba(123, 166, 212, 0.2);
            --accent: #7ec8ff;
            --muted: #8fa8c2;
            --error: #ff8b7f;
        }

        * {
            box-sizing: border-box;
        }

        html, body {
            width: 100%;
            height: 100%;
            margin: 0;
            background:
                radial-gradient(circle at top, rgba(49, 104, 189, 0.28), transparent 36%),
                linear-gradient(180deg, #0a1118 0%, #04070b 100%);
            color: #f5f9ff;
            font-family: "Segoe UI", "Microsoft YaHei UI", sans-serif;
            overflow: hidden;
        }

        body.error #bridge-message {
            color: var(--error);
        }

        #kuro-stream-surface {
            position: absolute;
            inset: 0;
            width: 100%;
            height: 100%;
            background: #000;
            cursor: auto;
        }

        #kuro-stream-surface * {
            cursor: inherit !important;
        }

        #bridge-overlay {
            position: absolute;
            inset: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            pointer-events: none;
            z-index: 20;
            transition: opacity 240ms ease;
        }

        #bridge-overlay.hidden {
            opacity: 0;
        }

        #bridge-card {
            width: min(560px, calc(100vw - 64px));
            padding: 28px 30px;
            border-radius: 22px;
            background: var(--panel);
            border: 1px solid var(--border);
            backdrop-filter: blur(22px);
            box-shadow: 0 24px 80px rgba(0, 0, 0, 0.45);
        }

        #bridge-tag {
            display: inline-flex;
            padding: 6px 10px;
            border-radius: 999px;
            background: rgba(37, 75, 117, 0.44);
            border: 1px solid rgba(126, 200, 255, 0.24);
            color: var(--accent);
            font-size: 12px;
            letter-spacing: 0.08em;
            text-transform: uppercase;
        }

        #bridge-title {
            margin: 18px 0 10px;
            font-size: 28px;
            font-weight: 700;
        }

        #bridge-message {
            margin: 0;
            color: var(--muted);
            font-size: 14px;
            line-height: 1.7;
        }

        #bridge-region {
            margin-top: 16px;
            color: #dbe7f5;
            font-size: 13px;
        }

        #bridge-progress {
            width: 100%;
            height: 6px;
            margin-top: 22px;
            overflow: hidden;
            border-radius: 999px;
            background: rgba(255, 255, 255, 0.08);
        }

        #bridge-progress::after {
            content: "";
            display: block;
            width: 32%;
            height: 100%;
            background: linear-gradient(90deg, #3a7eff 0%, #8dd7ff 100%);
            animation: kuro-progress 1.2s linear infinite;
            border-radius: inherit;
        }

        @keyframes kuro-progress {
            from { transform: translateX(-120%); }
            to { transform: translateX(360%); }
        }
    </style>
</head>
<body>
    <div id="kuro-stream-surface" tabindex="0"></div>
    <div id="bridge-overlay">
        <div id="bridge-card">
            <div id="bridge-tag">Native Stream Bridge</div>
            <div id="bridge-title">正在接入云端实例</div>
            <p id="bridge-message">原生业务层已完成开始游戏，正在初始化 Welink 串流 SDK。</p>
            <div id="bridge-region">节点：测试 | 会话：测试1</div>
            <div id="bridge-progress"></div>
        </div>
    </div>

    <script>
        (() => {
            let sdk = null;
            let isForeground = false;
            let lastNetworkStat = null;
            let mediaObserver = null;
            const audioState = {
                level: 1,
                muted: false
            };

            const enhancementState = {
                enabled: false
            };
            const payload = {
                scriptUrl: {{scriptUrlJson}},
                dispatchMessage: {{dispatchMessageJson}},
                bridgeConfig: {{bridgeConfigJson}},
                storageItems: {{storageItemsJson}}
            };

            enhancementState.enabled = Boolean(payload?.bridgeConfig?.enableImageEnhancement);

            const overlay = document.getElementById("bridge-overlay");
            const messageElement = document.getElementById("bridge-message");
            const surface = document.getElementById("kuro-stream-surface");

            const normalizeVolume = (percent) => {
                const parsed = Number(percent);
                if (!Number.isFinite(parsed)) {
                    return 100;
                }

                return Math.max(0, Math.min(100, Math.round(parsed)));
            };

            const applyMediaAudioState = (root = document) => {
                const volume = Math.max(0, Math.min(1, audioState.level));
                const mediaElements = [];

                if (root instanceof HTMLMediaElement) {
                    mediaElements.push(root);
                }

                if (typeof root?.querySelectorAll === "function") {
                    mediaElements.push(...root.querySelectorAll("video, audio"));
                }

                for (const mediaElement of mediaElements) {
                    try {
                        mediaElement.defaultMuted = audioState.muted;
                        mediaElement.muted = audioState.muted;
                        mediaElement.volume = volume;
                    } catch {
                    }
                }
            };

            const ensureMediaObserver = () => {
                if (mediaObserver || !document.documentElement) {
                    return;
                }

                mediaObserver = new MutationObserver((mutations) => {
                    for (const mutation of mutations) {
                        for (const node of mutation.addedNodes) {
                            if (node instanceof HTMLMediaElement) {
                                applyMediaAudioState(node);
                                continue;
                            }

                            if (node instanceof Element) {
                                applyMediaAudioState(node);
                            }
                        }
                    }
                });

                mediaObserver.observe(document.documentElement, {
                    childList: true,
                    subtree: true
                });
            };

            const setStreamVolume = (percent) => {
                const nextPercent = normalizeVolume(percent);
                audioState.level = nextPercent / 100;
                audioState.muted = nextPercent <= 0 ? true : audioState.muted;
                ensureMediaObserver();
                applyMediaAudioState(document);
                return { level: nextPercent, muted: audioState.muted };
            };

            const setStreamMuted = (muted) => {
                audioState.muted = Boolean(muted);
                ensureMediaObserver();
                applyMediaAudioState(document);
                return { level: Math.round(audioState.level * 100), muted: audioState.muted };
            };

            const applyImageEnhancement = (enabled) => {
                enhancementState.enabled = Boolean(enabled);

                if (!sdk) {
                    return { enabled: enhancementState.enabled, applied: false };
                }

                const officialMethod = typeof sdk?.openSuperResolution === "function"
                    ? {
                        name: "openSuperResolution",
                        args: [enhancementState.enabled]
                    }
                    : typeof sdk?.openEnhance === "function"
                        ? {
                            name: "openEnhance",
                            args: [enhancementState.enabled ? 2 : 0]
                        }
                        : null;

                if (!officialMethod) {
                    post("warning", {
                        message: "Welink SDK 未暴露官网使用的画质增强方法。",
                        enabled: enhancementState.enabled
                    });
                    return { enabled: enhancementState.enabled, applied: false };
                }

                try {
                    sdk[officialMethod.name](...officialMethod.args);
                    post("image-enhancement", {
                        message: enhancementState.enabled ? "画质增强已开启" : "画质增强已关闭",
                        enabled: enhancementState.enabled,
                        method: officialMethod.name
                    });
                    return { enabled: enhancementState.enabled, applied: true, method: officialMethod.name };
                } catch (error) {
                    post("warning", {
                        message: `调用 ${officialMethod.name} 切换画质增强失败。`,
                        enabled: enhancementState.enabled,
                        detail: error?.message || String(error)
                    });
                }

                return { enabled: enhancementState.enabled, applied: false };
            };

            const updateBridgeQualityConfig = (config) => {
                if (!config || typeof config !== "object") {
                    return payload.bridgeConfig;
                }

                payload.bridgeConfig = {
                    ...payload.bridgeConfig,
                    ...config
                };
                enhancementState.enabled = Boolean(payload.bridgeConfig.enableImageEnhancement);
                return payload.bridgeConfig;
            };

            const applyQualityProfile = (config, options = {}) => {
                const nextConfig = updateBridgeQualityConfig(config);

                if (!sdk) {
                    return { applied: false, config: nextConfig };
                }

                const noReport = Boolean(options.noReport);
                const targetBitRate = Number(nextConfig.bitRate) || 18000;
                const minBitRate = Number(nextConfig.bitRateMin) || 0;
                const maxBitRate = Number(nextConfig.bitRateMax) || targetBitRate;
                const streamStrategy = nextConfig.streamStrategy;

                if (typeof sdk?.setStreamStrategy === "function" && streamStrategy !== undefined && streamStrategy !== null) {
                    try {
                        sdk.setStreamStrategy(String(streamStrategy));
                    } catch {
                    }
                }

                if (typeof sdk?.setBitrateRange === "function" && minBitRate > 0 && maxBitRate > 0) {
                    try {
                        sdk.setBitrateRange(minBitRate, maxBitRate, noReport);
                    } catch {
                    }
                } else if (typeof sdk?.setBitrate === "function") {
                    try {
                        sdk.setBitrate(targetBitRate, noReport);
                    } catch {
                    }
                }

                if (typeof sdk?.setFps === "function") {
                    try {
                        sdk.setFps(nextConfig.fps || 60);
                    } catch {
                    }
                }

                applyImageEnhancement(enhancementState.enabled);
                setGameResolution();

                if (options.resendResolution) {
                    setTimeout(() => setGameResolution(), 3000);
                }

                return { applied: true, config: nextConfig };
            };

            const requestExit = () => {
                const candidateNames = ["exitGame", "stopGame", "closeGame", "endGame", "destroy"];

                for (const candidateName of candidateNames) {
                    if (typeof sdk?.[candidateName] !== "function") {
                        continue;
                    }

                    try {
                        sdk[candidateName]();
                        post("status", { message: `已调用 ${candidateName}，准备退出当前会话。` });
                        return { invoked: true, method: candidateName };
                    } catch (error) {
                        post("warning", {
                            message: `调用 ${candidateName} 退出会话失败。`,
                            detail: error?.message || String(error)
                        });
                    }
                }

                post("warning", { message: "Welink 未暴露可用的退出方法，将由宿主直接关闭窗口。" });
                return { invoked: false };
            };

            // Host-side controls exposed to the native window:
            // requestExit closes the stream session, setVolume/setMuted sync audio,
            // setImageEnhancement toggles the SDK's enhancement path,
            // and applyQualityProfile reapplies bitrate/FPS/resolution settings.
            window.__KURO_STREAM_CONTROL__ = {
                setVolume: setStreamVolume,
                setMuted: setStreamMuted,
                setImageEnhancement: applyImageEnhancement,
                applyQualityProfile,
                requestExit,
                getLastNetworkStat: () => lastNetworkStat
            };

            ensureMediaObserver();

            if (document.body) {
                document.body.tabIndex = 0;
            }

            const post = (type, extra = {}) => {
                try {
                    window.chrome?.webview?.postMessage({ type, ...extra });
                } catch {
                }
            };

            let focusSurfaceLock = false;
            const focusSurface = () => {
                if (focusSurfaceLock) {
                    return;
                }

                focusSurfaceLock = true;
                try {
                    surface?.focus?.();
                } catch {
                } finally {
                    queueMicrotask(() => {
                        focusSurfaceLock = false;
                    });
                }
            };

            let forcePointerLockAfterStreamReady = false;
            const tryForcePointerLockFromUserGesture = () => {
                if (!forcePointerLockAfterStreamReady || !payload?.bridgeConfig?.lockPoint) {
                    return;
                }

                if (document.pointerLockElement === surface) {
                    return;
                }

                try {
                    const result = surface?.requestPointerLock?.();
                    if (typeof result?.catch === "function") {
                        result.catch((error) => {
                            post("warning", {
                                message: "requestPointerLock 调用失败",
                                detail: error?.message || String(error)
                            });
                        });
                    }
                } catch (error) {
                    post("warning", {
                        message: "requestPointerLock 抛出异常",
                        detail: error?.message || String(error)
                    });
                }
            };

            const handlePointerActivation = () => {
                focusSurface();
                tryForcePointerLockFromUserGesture();
            };

            document.addEventListener("pointerlockchange", () => {
                post("pointer-lock", {
                    locked: document.pointerLockElement === surface,
                    lockPointEnabled: Boolean(payload?.bridgeConfig?.lockPoint)
                });
            }, true);

            document.addEventListener("pointerlockerror", () => {
                post("warning", {
                    message: "pointer lock 失败（浏览器拒绝或上下文不允许）"
                });
            }, true);

            const setMessage = (message, level = "info") => {
                if (messageElement) {
                    messageElement.textContent = message;
                }

                document.body.classList.toggle("error", level === "error");
                post("status", { message, level });
            };

            const parseJson = (value) => {
                if (!value || typeof value !== "string") {
                    return null;
                }

                try {
                    return JSON.parse(value);
                } catch {
                    return null;
                }
            };

            const launchState = payload.storageItems || window.__KURO_LAUNCH_OPTIONS__?.storageItems || {};
            let preopenState = 0;
            let keepAliveTimer = null;
            let lastPreLaunchResolution = null;

            const readStateItem = (key) => {
                try {
                    const localValue = window.localStorage?.getItem?.(key);
                    if (localValue) {
                        return localValue;
                    }
                } catch {
                }

                try {
                    const sessionValue = window.sessionStorage?.getItem?.(key);
                    if (sessionValue) {
                        return sessionValue;
                    }
                } catch {
                }

                const launchValue = launchState[key];
                return typeof launchValue === "string" && launchValue.length > 0
                    ? launchValue
                    : null;
            };

            const decodePayload = (value) => {
                try {
                    if (typeof value === "string") {
                        return value;
                    }

                    if (value instanceof ArrayBuffer) {
                        return new TextDecoder().decode(value);
                    }

                    if (ArrayBuffer.isView(value)) {
                        return new TextDecoder().decode(value);
                    }

                    return String(value ?? "");
                } catch {
                    return "";
                }
            };

            const getViewportResolution = () => ({
                width: Math.max(1, Math.round(surface?.clientWidth || window.innerWidth || 1280)),
                height: Math.max(1, Math.round(surface?.clientHeight || window.innerHeight || 720))
            });

            const clampEven = (value, min, max) => {
                const rounded = Number.isFinite(value) ? Math.round(value) : min;
                const clamped = Math.max(min, Math.min(max, rounded));
                if (clamped <= min) {
                    return min % 2 === 0 ? min : min + 1;
                }

                return clamped % 2 === 0 ? clamped : clamped - 1;
            };

            const normalizeResolution = (width, height) => ({
                width: clampEven(width, 640, 3840),
                height: clampEven(height, 360, 2160)
            });

            const getPhysicalResolution = () => {
                const viewport = getViewportResolution();
                const scale = Number(window.devicePixelRatio) > 0 ? Number(window.devicePixelRatio) : 1;
                const physical = normalizeResolution(viewport.width * scale, viewport.height * scale);
                return {
                    ...viewport,
                    ...physical,
                    scale
                };
            };

            const getSdkLoginInfo = () => {
                const appStore = parseJson(readStateItem("useMcCloudGameAppStore"));
                const directLoginInfo = parseJson(readStateItem("sdkLoginInfo"));
                return appStore?.sdkLoginInfo || directLoginInfo || null;
            };

            const getHeartbeatConfig = () => {
                const sdkConfig = parseJson(readStateItem("__KrSDK_SYS_CONF__")) || {};
                const enabled = Number(sdkConfig?.heartEnable ?? 1) !== 0;
                const frequencySeconds = Math.max(5, Number(sdkConfig?.heartFreq || 5) || 5);
                return {
                    enabled,
                    frequencySeconds,
                    intervalMs: frequencySeconds * 1000
                };
            };

            const getUserLoginInfo = () => {
                const sdkLoginInfo = getSdkLoginInfo();
                const traceId = readStateItem("useMcCloudGameDid") || "";

                if (!sdkLoginInfo?.cuid || !sdkLoginInfo?.token || !sdkLoginInfo?.username || !traceId) {
                    post("warning", {
                        message: "桥页登录态不完整，无法生成登录信息。",
                        detail: {
                            hasSdkLoginInfo: Boolean(sdkLoginInfo),
                            hasUid: Boolean(sdkLoginInfo?.cuid),
                            hasToken: Boolean(sdkLoginInfo?.token),
                            hasUserName: Boolean(sdkLoginInfo?.username),
                            hasTraceId: Boolean(traceId)
                        }
                    });
                    return null;
                }

                return {
                    LoginCode: 1,
                    Uid: sdkLoginInfo.cuid,
                    Token: sdkLoginInfo.token,
                    UserName: sdkLoginInfo.username,
                    TraceId: traceId
                };
            };

            const sendMessageWithKey = (key, data) => {
                if (!sdk || typeof sdk.sendDataToGameWithKey !== "function") {
                    post("warning", { message: `Welink 缺少 sendDataToGameWithKey，无法回传 ${key}` });
                    return false;
                }

                try {
                    sdk.sendDataToGameWithKey(data, key);
                    post("game-send", {
                        key,
                        message: typeof data === "string" ? data.slice(0, 400) : String(data)
                    });
                    return true;
                } catch (error) {
                    post("warning", {
                        message: `向游戏回传 ${key} 失败`,
                        key,
                        detail: error?.message || String(error)
                    });
                    return false;
                }
            };

            const sendUserData = () => {
                if (preopenState === 1) {
                    return false;
                }

                const loginInfo = getUserLoginInfo();
                if (!loginInfo) {
                    post("warning", { message: "缺少登录态，无法响应 RequestLogin。" });
                    return false;
                }

                return sendMessageWithKey("OnCloudGameLogin", JSON.stringify(loginInfo));
            };

            const sendGamePadDeviceChangeData = () => {
                return sendMessageWithKey("OnGamePadDeviceChange", JSON.stringify({
                    IsConnectPad: 0,
                    VendorId: 0,
                    ProductId: 0
                }));
            };

            const setGameResolution = (width, height) => {
                if (!sdk || typeof sdk.setGameResolution !== "function") {
                    return false;
                }

                const target = width > 0 && height > 0
                    ? normalizeResolution(width, height)
                    : getPhysicalResolution();
                const targetWidth = target.width;
                const targetHeight = target.height;

                try {
                    sdk.setGameResolution(targetWidth, targetHeight);
                    post("game-resolution", {
                        message: `${targetWidth}x${targetHeight}`,
                        width: targetWidth,
                        height: targetHeight,
                        viewportWidth: target.viewportWidth,
                        viewportHeight: target.viewportHeight,
                        scale: target.scale
                    });
                    return true;
                } catch (error) {
                    post("warning", {
                        message: `设置游戏分辨率失败: ${targetWidth}x${targetHeight}`,
                        detail: error?.message || String(error)
                    });
                    return false;
                }
            };

            const sendPreLaunchUserData = (screenResolution) => {
                if (preopenState !== 1) {
                    return false;
                }

                if (screenResolution?.width > 0 && screenResolution?.height > 0) {
                    lastPreLaunchResolution = {
                        width: screenResolution.width,
                        height: screenResolution.height
                    };
                }

                const loginInfo = getUserLoginInfo();
                if (!loginInfo) {
                    post("warning", { message: "缺少预开模式登录态，无法回传 OnCloudGameLoginPreLaunch。" });
                    return false;
                }

                const physical = getPhysicalResolution();
                const qualityFps = payload.bridgeConfig.fps || 60;
                const qualityDpi = Number(payload.bridgeConfig.dpi) || 120;
                const payloadData = {
                    TraceId: loginInfo.TraceId,
                    Platform: "Windows",
                    Fps: qualityFps,
                    Dpi: qualityDpi,
                    DeviceResolution: {
                        Width: physical.width,
                        Height: physical.height
                    },
                    ScreenResolution: {
                        Width: screenResolution?.width || physical.width,
                        Height: screenResolution?.height || physical.height
                    },
                    Device: navigator.userAgent,
                    LoginInfo: loginInfo
                };

                sendMessageWithKey("SetIsWebPlatform", "1");
                return sendMessageWithKey("OnCloudGameLoginPreLaunch", JSON.stringify(payloadData));
            };

            const sendKeepAlive = (reason) => {
                const heartbeatConfig = getHeartbeatConfig();
                if (!heartbeatConfig.enabled || !sdk) {
                    return false;
                }

                let sentWebPlatform = false;
                let sentLogin = false;

                if (preopenState === 1) {
                    sentLogin = sendPreLaunchUserData(lastPreLaunchResolution);
                    sentWebPlatform = sentLogin;
                } else {
                    sentWebPlatform = sendMessageWithKey("SetIsWebPlatform", "1");
                    sentLogin = sendUserData();
                }

                post("keepalive", {
                    reason,
                    preopenState,
                    sentWebPlatform,
                    sentLogin
                });

                return sentWebPlatform || sentLogin;
            };

            const startKeepAliveLoop = () => {
                const heartbeatConfig = getHeartbeatConfig();
                if (!heartbeatConfig.enabled || keepAliveTimer) {
                    return;
                }

                keepAliveTimer = window.setInterval(() => {
                    sendKeepAlive("timer");
                }, heartbeatConfig.intervalMs);

                post("keepalive-config", {
                    enabled: true,
                    frequencySeconds: heartbeatConfig.frequencySeconds
                });

                sendKeepAlive("initial");
            };

            const stopKeepAliveLoop = () => {
                if (!keepAliveTimer) {
                    return;
                }

                window.clearInterval(keepAliveTimer);
                keepAliveTimer = null;
                post("keepalive-stop", { message: "桥页保活已停止。" });
            };

            const handleGameMessage = (message) => {
                post("game-message", { message });

                switch (message) {
                    case "RequestLogin":
                        sendUserData();
                        break;
                    case "RequestGamePadDevice":
                        sendGamePadDeviceChangeData();
                        setGameResolution();
                        break;
                    case "HotPatchEnterGame":
                        setMessage("游戏热更已完成，正在进入游戏场景...");
                        setGameResolution();
                        break;
                    case "HotPatchExitGame":
                        setMessage("游戏仍在加载资源，等待进入游戏场景...");
                        break;
                }
            };

            const handleGameMessageWithKey = (key, message) => {
                post("game-message-with-key", { key, message });

                switch (key) {
                    case "InitPostWebView":
                        setGameResolution();
                        break;
                    case "ExitGame":
                        setMessage("云端游戏要求退出当前会话。", "error");
                        break;
                }
            };

            const handleSdkMessage = (code, detail) => {
                post("sdk-message", { code, detail });

                switch (Number(code)) {
                    case 6038:
                        if (typeof sdk?.setExitPointTimeout === "function") {
                            try {
                                sdk.setExitPointTimeout(100);
                            } catch {
                            }
                        }

                        applyQualityProfile(payload.bridgeConfig, { resendResolution: true });
                        hideOverlay();
                        focusSurface();
                        notifyForeground("sdk-first-video-frame");
                        post("first-frame");
                        break;
                    case 6252:
                        if (detail?.pipeState === 1) {
                            sendPreLaunchUserData();
                            setGameResolution();
                        }
                        break;
                    case 6253: {
                        const match = typeof detail === "string"
                            ? detail.match(/(\d+)x(\d+)/)
                            : null;

                        if (match) {
                            sendPreLaunchUserData({
                                width: Number(match[1]),
                                height: Number(match[2])
                            });
                            break;
                        }

                        sendPreLaunchUserData();
                        break;
                    }
                    case 6254:
                        if (detail?.type === "preopen" && typeof detail.info === "number") {
                            preopenState = detail.info;
                        }
                        break;
                }
            };

            const notifyForeground = (reason) => {
                if (isForeground) {
                    return;
                }

                if (!sdk || typeof sdk.onResume !== "function") {
                    return;
                }

                try {
                    isForeground = true;
                    sdk.onResume();
                    post("status", { message: `Welink 已切回前台状态: ${reason}` });
                } catch {
                    isForeground = false;
                }
            };

            const syncForegroundFlag = () => {
                isForeground = !document.hidden;
            };

            const notifyBackground = (reason) => {
                if (!isForeground) {
                    return;
                }

                if (!sdk || typeof sdk.onPause !== "function") {
                    return;
                }

                try {
                    isForeground = false;
                    sdk.onPause();
                    post("status", { message: `Welink 已切到后台状态: ${reason}` });
                } catch {
                    isForeground = true;
                }
            };

            const hideOverlay = () => {
                overlay?.classList.add("hidden");
                setTimeout(() => overlay?.remove(), 260);
            };

            window.addEventListener("pointerdown", focusSurface, true);
            window.addEventListener("mousedown", focusSurface, true);
            window.addEventListener("click", focusSurface, true);
            window.addEventListener("focus", () => {
                focusSurface();
                notifyForeground("window-focus");
            }, true);
            window.addEventListener("pageshow", () => {
                focusSurface();
                notifyForeground("pageshow");
            }, true);
            document.addEventListener("visibilitychange", () => {
                if (document.hidden) {
                    notifyBackground("document-hidden");
                    return;
                }

                focusSurface();
                notifyForeground("document-visible");
            }, true);
            window.addEventListener("beforeunload", stopKeepAliveLoop, true);
            window.addEventListener("pagehide", stopKeepAliveLoop, true);

            const loadScript = (source) => new Promise((resolve, reject) => {
                const element = document.createElement("script");
                element.src = source;
                element.async = true;
                element.onload = () => resolve();
                element.onerror = () => reject(new Error("WelinkCloudGame SDK 加载失败。"));
                document.head.appendChild(element);
            });

            const wrapMethod = (target, methodName, beforeInvoke) => {
                if (!target || typeof target[methodName] !== "function") {
                    syncForegroundFlag();
                    return;
                }

                const original = target[methodName].bind(target);
                target[methodName] = (...args) => {
                    try {
                        beforeInvoke(...args);
                    } catch {
                    }

                    return original(...args);
                };
            };

            window.addEventListener("error", (event) => {
                const message = event?.message || "桥页脚本执行失败。";
                setMessage(message, "error");
                post("error", { message });
            });

            window.addEventListener("unhandledrejection", (event) => {
                const message = event?.reason?.message || String(event?.reason || "桥页出现未处理异常。");
                setMessage(message, "error");
                post("error", { message });
            });

            (async () => {
                setMessage("正在加载 WelinkCloudGame SDK...");
                await loadScript(payload.scriptUrl);

                if (typeof window.WelinkCloudGame !== "function") {
                    throw new Error("WelinkCloudGame 不可用，无法启动串流。 ");
                }

                const q = payload.bridgeConfig;
                const initialResolution = getPhysicalResolution();
                const sdkInitConfig = {
                    ...q,
                    videoWidth: initialResolution.width,
                    videoHeight: initialResolution.height,
                    enableCheckMobileDesktopMode: true,
                    rotate: 0
                };

                sdk = new window.WelinkCloudGame(sdkInitConfig);
                window.__KURO_STREAM_SDK__ = sdk;
                window.WLCG = sdk;

                if (typeof sdk.onGameData !== "undefined") {
                    sdk.onGameData = (data) => {
                        handleGameMessage(decodePayload(data));
                    };
                }

                if (typeof sdk.onGameDataWithKey !== "undefined") {
                    sdk.onGameDataWithKey = (key, data) => {
                        handleGameMessageWithKey(key, decodePayload(data));
                    };
                }

                if (typeof sdk.startGameInfo !== "undefined") {
                    sdk.startGameInfo = (code, detail) => {
                        handleSdkMessage(code, detail);
                    };
                }

                if (typeof sdk.startGameError !== "undefined") {
                    sdk.startGameError = (code, detail) => {
                        const suffix = detail ? ` | ${typeof detail === "string" ? detail : JSON.stringify(detail)}` : "";
                        post("warning", {
                            message: `Welink startGameError: code=${code}${suffix}`,
                            code,
                            detail
                        });
                    };
                }

                if (typeof sdk.showGameStatisticsData !== "undefined") {
                    sdk.showGameStatisticsData = (detail) => {
                        lastNetworkStat = detail || null;
                        post("network-stat", { detail });
                    };
                }

                if (typeof sdk.openAutoReconnectServer === "function") {
                    try {
                        sdk.openAutoReconnectServer(true);
                    } catch {
                    }
                }

                if (typeof sdk.setGameScreenAdaptation === "function") {
                    try {
                        sdk.setGameScreenAdaptation(true);
                    } catch {
                    }
                }

                wrapMethod(sdk, "onFirstVideoFrame", () => {
                    if (typeof sdk.gameVideoPlay === "function") {
                        try {
                            sdk.gameVideoPlay();
                        } catch {
                        }
                    }

                    if (typeof sdk.unblockKeyboard === "function") {
                        try {
                            sdk.unblockKeyboard();
                        } catch {
                        }
                    }

                    if (typeof sdk.unblockMouse === "function") {
                        try {
                            sdk.unblockMouse();
                        } catch {
                        }
                    }

                    applyQualityProfile(payload.bridgeConfig);

                    applyMediaAudioState(document);
                    forcePointerLockAfterStreamReady = true;
                    hideOverlay();
                    focusSurface();
                    notifyForeground("first-video-frame");
                    post("first-frame");
                });

                wrapMethod(sdk, "handleStartGameError", (code, detail, notThrow) => {
                    const suffix = detail ? ` | ${typeof detail === "string" ? detail : JSON.stringify(detail)}` : "";
                    const message = `Welink 协商中出现可恢复告警: code=${code}${suffix}`;
                    post("warning", { message, code, detail, notThrow: Boolean(notThrow) });
                });

                wrapMethod(sdk, "handlePivotalAction", (name, info) => {
                    post("pivotal", { name, info });
                });

                setMessage("SDK 已加载，正在初始化 Welink 实例...");
                await sdk.init();
                focusSurface();
                syncForegroundFlag();
                notifyForeground("sdk-init");

                if (typeof sdk.unblockKeyboard === "function") {
                    try {
                        sdk.unblockKeyboard();
                    } catch {
                    }
                }

                if (typeof sdk.unblockMouse === "function") {
                    try {
                        sdk.unblockMouse();
                    } catch {
                    }
                }

                setGameResolution();

                setMessage("Welink 已初始化，正在连接云端实例...");
                await sdk.startGame(payload.dispatchMessage);
                focusSurface();
                syncForegroundFlag();
                notifyForeground("start-game-dispatched");
                setGameResolution();
                startKeepAliveLoop();

                setMessage("串流启动请求已发送，等待首帧到达...");
                post("launch-dispatched");
            })().catch((error) => {
                const message = error?.message || String(error || "串流桥页启动失败。");
                setMessage(message, "error");
                post("error", { message });
            });
        })();
    </script>
</body>
</html>
""";
        }

        private void ApplyCookies(CoreWebView2CookieManager cookieManager)
        {
            if (Option.Cookies.Count == 0)
            {
                return;
            }

            var domain = Option.CookieDomain;
            var isSecure =
                Uri.TryCreate(
                    "https://mc.kurogames.com/cloud/index.html",
                    UriKind.Absolute,
                    out var uri
                )
                && string.Equals(
                    uri.Scheme,
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase
                );

            foreach (var pair in Option.Cookies)
            {
                var cookie = cookieManager.CreateCookie(pair.Key, pair.Value, domain, "/");
                cookie.IsSecure = isSecure;
                cookie.IsHttpOnly = false;
                cookie.SameSite = CoreWebView2CookieSameSiteKind.None;
                cookieManager.AddOrUpdateCookie(cookie);
            }
        }

        /// <summary>
        /// 隐藏系统光标并在运行时持续压制，防止 WinUI XAML 层重新显示光标。
        /// </summary>
        private void HideSystemCursor()
        {
            if (_cursorHidden)
            {
                return;
            }

            _cursorHidden = true;

            TryInstallWebViewCursorSubclass();
            OverrideSystemCursorsWithTransparent();
            EnsureSystemCursorHidden();

            if (_cursorTimer is null)
            {
                _cursorTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                _cursorTimer.Tick += (_, _) =>
                {
                    if (_cursorHidden && !_webViewCursorSubclassInstalled)
                    {
                        TryInstallWebViewCursorSubclass();
                    }

                    if (_cursorHidden && IsSystemCursorVisible())
                    {
                        EnsureSystemCursorHidden();
                    }
                };
            }

            _cursorTimer.Start();
        }

        private void ShowSystemCursor()
        {
            if (!_cursorHidden)
            {
                return;
            }

            _cursorHidden = false;
            _cursorTimer?.Stop();

            RestoreSystemCursors();
            while (ShowCursor(true) < 0) { }
        }

        private static void EnsureSystemCursorHidden()
        {
            while (ShowCursor(false) >= 0) { }
        }

        private void OverrideSystemCursorsWithTransparent()
        {
            if (_systemCursorSchemeOverridden)
            {
                return;
            }

            foreach (var cursorId in SystemCursorIds)
            {
                var transparentCursor = CreateTransparentCursorHandle();
                if (transparentCursor != IntPtr.Zero)
                {
                    _ = SetSystemCursor(transparentCursor, cursorId);
                }
            }

            _systemCursorSchemeOverridden = true;
        }

        private void RestoreSystemCursors()
        {
            if (!_systemCursorSchemeOverridden)
            {
                return;
            }

            _ = SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, 0);
            _systemCursorSchemeOverridden = false;
        }

        private static IntPtr CreateTransparentCursorHandle()
        {
            byte[] andMask = [0xFF, 0xFF, 0xFF, 0xFF];
            byte[] xorMask = [0x00, 0x00, 0x00, 0x00];
            return CreateCursor(IntPtr.Zero, 0, 0, 1, 1, andMask, xorMask);
        }

        private void ToggleSystemCursor()
        {
            if (_cursorHidden)
            {
                ShowSystemCursor();
                return;
            }

            HideSystemCursor();
        }

        private void EnsureWindowMessageSubclass()
        {
            if (_windowHandle == IntPtr.Zero)
            {
                _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
                if (_windowHandle == IntPtr.Zero)
                {
                    return;
                }
            }

            if (!_windowMessageSubclassInstalled)
            {
                _windowMessageSubclassProc ??= WindowMessageSubclassProc;
                _windowMessageSubclassInstalled = SetWindowSubclass(
                    _windowHandle,
                    _windowMessageSubclassProc,
                    new UIntPtr(WindowMessageSubclassId),
                    UIntPtr.Zero
                );
            }

            if (!_cursorHotKeyRegistered)
            {
                _cursorHotKeyRegistered = RegisterHotKey(
                    _windowHandle,
                    HotKeyIdToggleCursor,
                    MOD_ALT,
                    VK_Q
                );
            }
        }

        private void RemoveWindowMessageSubclass()
        {
            if (_cursorHotKeyRegistered && _windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HotKeyIdToggleCursor);
                _cursorHotKeyRegistered = false;
            }

            if (_windowMessageSubclassInstalled && _windowHandle != IntPtr.Zero)
            {
                RemoveWindowSubclass(
                    _windowHandle,
                    _windowMessageSubclassProc,
                    new UIntPtr(WindowMessageSubclassId)
                );
                _windowMessageSubclassInstalled = false;
            }
        }

        private void TryInstallWebViewCursorSubclass()
        {
            if (_webViewCursorSubclassInstalled)
            {
                return;
            }

            if (_windowHandle == IntPtr.Zero)
            {
                _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
                if (_windowHandle == IntPtr.Zero)
                {
                    return;
                }
            }

            _webViewChildHandle = FindWebViewChildWindow(_windowHandle);
            if (_webViewChildHandle == IntPtr.Zero)
            {
                return;
            }

            _webViewCursorSubclassProc ??= WebViewCursorSubclassProc;
            _webViewCursorSubclassInstalled = SetWindowSubclass(
                _webViewChildHandle,
                _webViewCursorSubclassProc,
                UIntPtr.Zero,
                UIntPtr.Zero
            );
        }

        private void RemoveWebViewCursorSubclass()
        {
            if (!_webViewCursorSubclassInstalled || _webViewChildHandle == IntPtr.Zero)
            {
                return;
            }

            RemoveWindowSubclass(_webViewChildHandle, _webViewCursorSubclassProc, UIntPtr.Zero);
            _webViewCursorSubclassInstalled = false;
            _webViewChildHandle = IntPtr.Zero;
        }

        private IntPtr FindWebViewChildWindow(IntPtr parentWindowHandle)
        {
            IntPtr result = IntPtr.Zero;

            EnumChildWindows(
                parentWindowHandle,
                (childHandle, _) =>
                {
                    if (IsWebViewWindowClass(childHandle))
                    {
                        result = childHandle;
                        return false;
                    }

                    var descendant = FindWebViewChildWindow(childHandle);
                    if (descendant != IntPtr.Zero)
                    {
                        result = descendant;
                        return false;
                    }

                    return true;
                },
                IntPtr.Zero
            );

            return result;
        }

        private static bool IsWebViewWindowClass(IntPtr windowHandle)
        {
            var classNameBuilder = new StringBuilder(256);
            _ = GetClassName(windowHandle, classNameBuilder, classNameBuilder.Capacity);
            var className = classNameBuilder.ToString();

            return className.StartsWith("Chrome_WidgetWin_", StringComparison.Ordinal)
                || className.Contains("WebView", StringComparison.OrdinalIgnoreCase);
        }

        private IntPtr WebViewCursorSubclassProc(
            IntPtr windowHandle,
            uint message,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr subclassId,
            UIntPtr referenceData
        )
        {
            if (_cursorHidden && message == WM_SETCURSOR)
            {
                SetCursor(IntPtr.Zero);
                return new IntPtr(1);
            }

            return DefSubclassProc(windowHandle, message, wParam, lParam);
        }

        private IntPtr WindowMessageSubclassProc(
            IntPtr windowHandle,
            uint message,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr subclassId,
            UIntPtr referenceData
        )
        {
            if (message == WM_HOTKEY && wParam == new IntPtr(HotKeyIdToggleCursor))
            {
                ToggleSystemCursor();
                return new IntPtr(1);
            }

            return DefSubclassProc(windowHandle, message, wParam, lParam);
        }

        private static bool IsSystemCursorVisible()
        {
            var cursorInfo = new CURSORINFO
            {
                cbSize = Marshal.SizeOf<CURSORINFO>()
            };

            return GetCursorInfo(out cursorInfo)
                && (cursorInfo.flags & CURSOR_SHOWING) == CURSOR_SHOWING;
        }

        private const int CURSOR_SHOWING = 0x00000001;
        private const int HotKeyIdToggleCursor = 0x5141;
        private const uint WindowMessageSubclassId = 0x5141;
        private const uint MOD_ALT = 0x0001;
        private const uint SPI_SETCURSORS = 0x0057;
        private const uint WM_HOTKEY = 0x0312;
        private const uint WM_SETCURSOR = 0x0020;
        private const uint VK_Q = 0x51;

        private static readonly uint[] SystemCursorIds =
        [
            32512, // OCR_NORMAL
            32513, // OCR_IBEAM
            32514, // OCR_WAIT
            32515, // OCR_CROSS
            32516, // OCR_UP
            32642, // OCR_SIZENWSE
            32643, // OCR_SIZENESW
            32644, // OCR_SIZEWE
            32645, // OCR_SIZENS
            32646, // OCR_SIZEALL
            32648, // OCR_NO
            32649, // OCR_HAND
            32650, // OCR_APPSTARTING
            32651, // OCR_HELP
            32671, // OCR_PIN
            32672, // OCR_PERSON
        ];

        private delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);

        private delegate IntPtr SUBCLASSPROC(
            IntPtr windowHandle,
            uint message,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr subclassId,
            UIntPtr referenceData
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateCursor(
            IntPtr hInst,
            int xHotSpot,
            int yHotSpot,
            int nWidth,
            int nHeight,
            byte[] pvANDPlane,
            byte[] pvXORPlane
        );

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            IntPtr pvParam,
            uint fWinIni
        );

        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(
            IntPtr hWnd,
            StringBuilder lpClassName,
            int nMaxCount
        );

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(
            IntPtr hWndParent,
            EnumWindowsProc lpEnumFunc,
            IntPtr lParam
        );

        [DllImport("comctl32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowSubclass(
            IntPtr hWnd,
            SUBCLASSPROC pfnSubclass,
            UIntPtr uIdSubclass,
            UIntPtr dwRefData
        );

        [DllImport("comctl32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveWindowSubclass(
            IntPtr hWnd,
            SUBCLASSPROC pfnSubclass,
            UIntPtr uIdSubclass
        );

        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern IntPtr DefSubclassProc(
            IntPtr hWnd,
            uint uMsg,
            IntPtr wParam,
            IntPtr lParam
        );

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);
    }
}
