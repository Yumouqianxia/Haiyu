using System.Net.WebSockets;
using ChromeCDPSharp.Common;
using ChromeCDPSharp.Models;
using ChromeCDPSharp.Serialization;
using Waves.Core.Common;
using Windows.ApplicationModel.DataTransfer;
using Windows.Security.Credentials.UI;
using ZXing.Aztec.Internal;

namespace Haiyu.ViewModel.ToolkitsViewModel;

public partial class AutoKuroTokenViewModel : ViewModelBase
{
    private const string RoleListApi = "https://api.kurobbs.com/aki/widget/getData";

    private readonly AdbClient _adbClient = new();
    private readonly List<IDisposable> _networkSubscriptions = [];
    private readonly HashSet<string> _trackedRequestIds = [];

    private CDPClient? _cdpClient;
    private string? _webSocketDebuggerUrl;
    private string? _lastReadableResponseRequestId;
    private Dictionary<string, object?>? _requestHeader;

    public AutoKuroTokenViewModel(IPickersService pickersService)
    {
        PickerService = pickersService;
    }

    public IPickersService PickerService { get; }

    public Window Window { get; internal set; }

    [ObservableProperty]
    public partial string AdbPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Port { get; set; } = 9222;

    [ObservableProperty]
    public partial ObservableCollection<AdbDeviceInfo> Devices { get; private set; } = [];

    [ObservableProperty]
    public partial AdbDeviceInfo? SelectDevice { get; set; }

    [ObservableProperty]
    public partial WebSocketState WebSocketState { get; set; }

    [ObservableProperty]
    public partial CdpConnectionState CdpState { get; set; }

    [ObservableProperty]
    public partial string LogText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Did { get; set; }

    [ObservableProperty]
    public partial string Token { get; set; }

    [ObservableProperty]
    public partial string PlayerId { get; set; }

    [RelayCommand]
    public async Task SelectAdbPathAsync()
    {
        var openFile = await PickerService.GetFileOpenPicker([".exe"]);
        if (
            openFile is null
            || !openFile.Path.Contains("adb.exe", StringComparison.OrdinalIgnoreCase)
        )
        {
            return;
        }

        AdbPath = openFile.Path;
        _adbClient.InitAdbServer(AdbPath);
    }

    [RelayCommand]
    public async Task RefreshDeviceAsync()
    {
        Devices = (await _adbClient.GetDevicesAsync(CTS.Token)).ToObservableCollection();
    }

    [RelayCommand]
    public async Task AutoConnectAsync()
    {
        if (SelectDevice is null)
        {
            AppendLog("请先选择一个安卓设备。");
            return;
        }

        var sockets = await _adbClient.GetWebViewSocketsAsync(SelectDevice.Serial);
        if (sockets.Count == 0)
        {
            AppendLog("未找到 WebView 调试 Socket。");
            return;
        }

        var socket = sockets[0];
        _webSocketDebuggerUrl = await _adbClient.GetWebSocketDebuggerUrlAsync(
            SelectDevice.Serial,
            socket.SocketName,
            Port,
            CTS.Token
        );
        await ConnectCdpClientAsync(_webSocketDebuggerUrl);
    }

    [RelayCommand]
    public async Task ManualReconnectAsync()
    {
        if (_cdpClient is null)
        {
            if (string.IsNullOrWhiteSpace(_webSocketDebuggerUrl))
            {
                AppendLog("请先连接一次 CDP。");
                return;
            }

            await ConnectCdpClientAsync(_webSocketDebuggerUrl);
            return;
        }

        AppendLog("正在重连 CDP...");
        await _cdpClient.ReconnectAsync(CTS.Token);
        ResetTrackedRequests();
        AppendLog("CDP 已重连。");
    }

    [RelayCommand]
    public async Task StartTrafficMonitorAsync()
    {
        if (!IsCdpConnected())
        {
            AppendLog("CDP 未连接，请先连接或手动重连。");
            return;
        }

        ClearNetworkSubscriptions();
        ResetTrackedRequests();

        _networkSubscriptions.Add(
            _cdpClient!.Subscribe<RequestWillBeSentEvent>(
                "Network.requestWillBeSent",
                CdpJsonContext.Default.RequestWillBeSentEvent,
                e =>
                {
                    if (IsTargetUrl(e.Request.Url))
                    {
                        _requestHeader = e.Request.Headers;
                        if (_requestHeader.TryGetValue("did", out var did))
                        {
                            this.Window.DispatcherQueue.TryEnqueue(() =>
                            {
                                this.Did = did.ToString();
                            });
                        }
                        if (_requestHeader.TryGetValue("token", out var token))
                        {
                            this.Window.DispatcherQueue.TryEnqueue(() =>
                            {
                                this.Token = token.ToString();
                            });
                        }
                        AppendLog($"捕获请求: {e.Request.Method} {e.Request.Url}");
                    }

                    return ValueTask.CompletedTask;
                }
            )
        );
        _networkSubscriptions.Add(
            _cdpClient.Subscribe<ResponseReceivedEvent>(
                "Network.responseReceived",
                CdpJsonContext.Default.ResponseReceivedEvent,
                e =>
                {
                    if (IsTargetUrl(e.Response.Url))
                    {
                        _trackedRequestIds.Add(e.RequestId);
                        AppendLog($"响应头已到达: {e.Response.Status} {e.Response.Url}");
                    }

                    return ValueTask.CompletedTask;
                }
            )
        );
        _networkSubscriptions.Add(
            _cdpClient.Subscribe<LoadingFinishedEvent>(
                "Network.loadingFinished",
                CdpJsonContext.Default.LoadingFinishedEvent,
                async e =>
                {
                    if (_trackedRequestIds.Remove(e.RequestId))
                    {
                        _lastReadableResponseRequestId = e.RequestId;
                        var result = await _cdpClient!.SendCommandAsync(
                            "Network.getResponseBody",
                            new GetResponseBodyParams(e.RequestId),
                            CdpJsonContext.Default.GetResponseBodyParams,
                            CdpJsonContext.Default.CdpCommandResponseGetResponseBodyResult,
                            CTS.Token
                        );
                        var jsonO = JsonObject.Parse(result.Body);
                        var playerId = jsonO["data"]["userId"];
                        this.Window.DispatcherQueue.TryEnqueue(() =>
                        {
                            this.PlayerId = playerId.ToString();
                        });
                    }
                }
            )
        );
        _networkSubscriptions.Add(
            _cdpClient.Subscribe<LoadingFailedEvent>(
                "Network.loadingFailed",
                CdpJsonContext.Default.LoadingFailedEvent,
                e =>
                {
                    if (_trackedRequestIds.Remove(e.RequestId))
                    {
                        AppendLog($"响应失败，无法读取 Body: {e.ErrorText}");
                    }

                    return ValueTask.CompletedTask;
                }
            )
        );

        await _cdpClient.SendCommandAsync(
            "Network.enable",
            new NetworkEnableParams(),
            CdpJsonContext.Default.NetworkEnableParams,
            CdpJsonContext.Default.CdpCommandResponseEmptyResult,
            CTS.Token
        );

        AppendLog("已开始监控 Network 流量。");
    }

    [RelayCommand]
    public async Task ReadResponseBodyAsync()
    {
        if (!IsCdpConnected())
        {
            AppendLog("CDP 未连接，请先连接或手动重连。");
            return;
        }

        if (string.IsNullOrWhiteSpace(_lastReadableResponseRequestId))
        {
            AppendLog("还没有已完成的响应体，请先触发目标请求并等待 loadingFinished。");
            return;
        }

        var result = await _cdpClient!.SendCommandAsync(
            "Network.getResponseBody",
            new GetResponseBodyParams(_lastReadableResponseRequestId),
            CdpJsonContext.Default.GetResponseBodyParams,
            CdpJsonContext.Default.CdpCommandResponseGetResponseBodyResult,
            CTS.Token
        );

        var body = result.Base64Encoded ? $"[Base64]{result.Body}" : result.Body;
        AppendLog($"Body({body.Length}): {body}");
    }

    private async Task ConnectCdpClientAsync(string webSocketDebuggerUrl)
    {
        ClearNetworkSubscriptions();
        if (_cdpClient is not null)
        {
            _cdpClient.ConnectionStateChanged -= OnCdpClientConnectionStateChanged;
            await _cdpClient.DisposeAsync();
        }

        ResetTrackedRequests();
        _cdpClient = new CDPClient(webSocketDebuggerUrl);
        _cdpClient.ConnectionStateChanged += OnCdpClientConnectionStateChanged;
        await _cdpClient.ConnectAsync(CTS.Token);
        AppendLog($"CDP 已连接: {webSocketDebuggerUrl}");
    }

    private void OnCdpClientConnectionStateChanged(
        object? sender,
        CdpConnectionStateChangedEventArgs e
    )
    {
        Window.DispatcherQueue.TryEnqueue(() =>
        {
            WebSocketState = e.WebSocketState;
            CdpState = e.CurrentState;
        });
    }

    private static bool IsTargetUrl(string url)
    {
        return url.Contains(RoleListApi, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsCdpConnected()
    {
        return _cdpClient is not null && _cdpClient.ConnectionState == CdpConnectionState.Connected;
    }

    private void ResetTrackedRequests()
    {
        _trackedRequestIds.Clear();
        _lastReadableResponseRequestId = null;
        _requestHeader = null;
    }

    private void AppendLog(string message)
    {
        Window.DispatcherQueue.TryEnqueue(() =>
        {
            LogText = $"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}{LogText}";
        });
    }

    private void ClearNetworkSubscriptions()
    {
        foreach (var subscription in _networkSubscriptions)
        {
            subscription.Dispose();
        }

        _networkSubscriptions.Clear();
    }

    [RelayCommand]
    async Task CopySession()
    {
        var result = await UserConsentVerifier.RequestVerificationAsync(
            "复制这些信息需要你进行二次确认"
        );
        if (result == UserConsentVerificationResult.Verified)
        {
            var package = new DataPackage();
            package.SetText($"""
            Did:{this.Did}
            Token:{this.Token}
            PlayerId:{this.PlayerId}
            """);
            Clipboard.SetContent(package);
        }
        
    }

    public override void Dispose()
    {
        ClearNetworkSubscriptions();
        if (_cdpClient is not null)
        {
            _cdpClient.ConnectionStateChanged -= OnCdpClientConnectionStateChanged;
            _ = _cdpClient.DisposeAsync();
        }

        base.Dispose();
    }
}
