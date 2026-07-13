using System.Collections.Concurrent;
using System.Buffers;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using Console = System.Console;

namespace AndroidWebViewNetworkMonitor;

internal static class ProgramEntry
{
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var options = MonitorOptions.Parse(args);
            if (options.ShowHelp)
            {
                PrintHelp();
                return 0;
            }

            using var cancellationSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationSource.Cancel();
            };

            var adbClient = new AdbClient();
            var selector = new WebViewTargetSelector(adbClient);
            var targetSelection = await selector.SelectAsync(options, cancellationSource.Token);
            try
            {
                using var logWriter = new JsonLogWriter(options.LogFilePath);
                Console.WriteLine($"[info] device: {targetSelection.DeviceSerial}");
                Console.WriteLine($"[info] socket: {targetSelection.SocketName}");
                Console.WriteLine($"[info] port: {targetSelection.Port}");
                Console.WriteLine($"[info] target: {targetSelection.Target.Title} ({targetSelection.Target.Url})");
                Console.WriteLine($"[info] log file: {options.LogFilePath}");

                await logWriter.WriteSessionAsync(new MonitorSessionStartedLogEntry(
                    "session_started",
                    DateTimeOffset.Now,
                    targetSelection.DeviceSerial,
                    targetSelection.SocketName,
                    targetSelection.Port,
                    targetSelection.Target.Title,
                    targetSelection.Target.Url,
                    options.UrlMatchKeyword,
                    options.CaptureBody,
                    options.ReloadAfterAttach,
                    options.BodyPreviewMaxBytes));

                using var protocolClient = new AotCdpClient(new Uri(targetSelection.Target.WebSocketDebuggerUrl));
                var monitor = new NetworkMonitor(protocolClient, logWriter);
                await monitor.RunAsync(options, cancellationSource.Token);
                return 0;
            }
            finally
            {
                await adbClient.RemoveForwardAsync(targetSelection.DeviceSerial, targetSelection.Port, CancellationToken.None);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[info] canceled");
            return 130;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[error] {ex.Message}");
            return 1;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Android WebView Network Monitor");
        Console.WriteLine("Usage:");
        Console.WriteLine("  AndroidWebViewNetworkMonitor [--socket <name>] [--port <number>] [--match <keyword>] [--body] [--reload] [--body-preview-kb <number>] [--log-file <path>]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --socket <name>           Use a specific webview_devtools_remote socket.");
        Console.WriteLine("  --port <number>           Local forwarded TCP port. Default: 9222.");
        Console.WriteLine("  --match <keyword>         Only log requests whose URL contains the keyword.");
        Console.WriteLine("  --body                    Capture response body previews for XHR, Fetch, and Document.");
        Console.WriteLine("  --reload                  Reload after monitoring is enabled, bypassing cache.");
        Console.WriteLine("  --body-preview-kb <n>     Limit preview size in KiB. Default: 16.");
        Console.WriteLine("  --log-file <path>         Write structured JSONL logs to the given file.");
        Console.WriteLine("  --help                    Show this help message.");
        Console.WriteLine();
        Console.WriteLine("Requirements:");
        Console.WriteLine("  1. Android device connected over USB with adb enabled.");
        Console.WriteLine("  2. Target app has WebView.setWebContentsDebuggingEnabled(true).");
        Console.WriteLine("  3. The sample will use .tools/platform-tools/adb.exe first, then fall back to adb on PATH.");
    }
}

internal sealed record MonitorOptions(
    string? SocketName,
    int Port,
    string? UrlMatchKeyword,
    bool CaptureBody,
    bool ReloadAfterAttach,
    int BodyPreviewMaxBytes,
    string LogFilePath,
    bool ShowHelp)
{
    public static MonitorOptions Parse(string[] args)
    {
        string? socketName = null;
        int port = 9222;
        string? match = null;
        bool captureBody = false;
        bool reloadAfterAttach = false;
        int bodyPreviewMaxBytes = 16 * 1024;
        string logFilePath = BuildDefaultLogFilePath();
        bool showHelp = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--socket":
                    socketName = ReadValue(args, ref i, "--socket");
                    break;
                case "--port":
                    port = int.Parse(ReadValue(args, ref i, "--port"));
                    break;
                case "--match":
                    match = ReadValue(args, ref i, "--match");
                    break;
                case "--body":
                    captureBody = true;
                    break;
                case "--reload":
                    reloadAfterAttach = true;
                    break;
                case "--body-preview-kb":
                    bodyPreviewMaxBytes = int.Parse(ReadValue(args, ref i, "--body-preview-kb")) * 1024;
                    break;
                case "--log-file":
                    logFilePath = ReadValue(args, ref i, "--log-file");
                    break;
                case "--help":
                case "-h":
                case "/?":
                    showHelp = true;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown argument: {args[i]}");
            }
        }

        if (port is < 1 or > 65535)
        {
            throw new InvalidOperationException("--port must be between 1 and 65535.");
        }

        if (bodyPreviewMaxBytes <= 0)
        {
            throw new InvalidOperationException("--body-preview-kb must be greater than 0.");
        }

        return new MonitorOptions(socketName, port, match, captureBody, reloadAfterAttach, bodyPreviewMaxBytes, Path.GetFullPath(logFilePath), showHelp);
    }

    private static string ReadValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new InvalidOperationException($"Missing value for {optionName}.");
        }

        index++;
        return args[index];
    }

    private static string BuildDefaultLogFilePath()
    {
        string logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        string fileName = $"monitor-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.jsonl";
        return Path.Combine(logsDirectory, fileName);
    }
}

internal sealed class AdbClient
{
    private readonly string _adbExecutablePath;

    public AdbClient()
    {
        _adbExecutablePath = ResolveAdbExecutablePath();
    }

    public async Task<string> GetSingleDeviceSerialAsync(CancellationToken cancellationToken)
    {
        var result = await RunAdbAsync("devices", cancellationToken);
        EnsureSuccess(result, "Failed to list adb devices.");

        var devices = result.StandardOutput
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(static line => line.Trim())
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(static line => line.Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries))
            .Where(static parts => parts.Length >= 2 && parts[1] == "device")
            .Select(static parts => parts[0])
            .ToArray();

        return devices.Length switch
        {
            0 => throw new InvalidOperationException("No Android device is online in adb."),
            > 1 => throw new InvalidOperationException("Multiple adb devices are connected. First version supports exactly one device."),
            _ => devices[0]
        };
    }

    public async Task<IReadOnlyList<WebViewSocketInfo>> GetWebViewSocketsAsync(string deviceSerial, CancellationToken cancellationToken)
    {
        var result = await RunAdbAsync($"-s {Quote(deviceSerial)} shell cat /proc/net/unix", cancellationToken);
        EnsureSuccess(result, "Failed to read /proc/net/unix from the device.");

        var sockets = result.StandardOutput
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseSocketLine)
            .Where(static socket => socket is not null)
            .DistinctBy(static socket => socket!.SocketName, StringComparer.Ordinal)
            .Cast<WebViewSocketInfo>()
            .ToArray();

        if (sockets.Length == 0)
        {
            throw new InvalidOperationException("No debuggable Android WebView sockets were found. Make sure the app enabled WebView debugging.");
        }

        return sockets;
    }

    public async Task ForwardAsync(string deviceSerial, int localPort, string socketName, CancellationToken cancellationToken)
    {
        await RunAndEnsureAsync($"-s {Quote(deviceSerial)} forward tcp:{localPort} localabstract:{Quote(socketName)}", "Failed to forward adb port.", cancellationToken);
    }

    public async Task RemoveForwardAsync(string deviceSerial, int localPort, CancellationToken cancellationToken)
    {
        await RunAdbAsync($"-s {Quote(deviceSerial)} forward --remove tcp:{localPort}", cancellationToken);
    }

    private async Task RunAndEnsureAsync(string arguments, string message, CancellationToken cancellationToken)
    {
        var result = await RunAdbAsync(arguments, cancellationToken);
        EnsureSuccess(result, message);
    }

    private static WebViewSocketInfo? ParseSocketLine(string line)
    {
        int atIndex = line.IndexOf('@');
        if (atIndex < 0)
        {
            return null;
        }

        string socketName = line[(atIndex + 1)..].Trim();
        if (!socketName.Contains("webview_devtools_remote", StringComparison.Ordinal))
        {
            return null;
        }

        string processPart = socketName[(socketName.LastIndexOf('_') + 1)..];
        _ = int.TryParse(processPart, out int pid);
        return new WebViewSocketInfo(socketName, pid);
    }

    private async Task<CommandResult> RunAdbAsync(string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _adbExecutablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to start adb from '{_adbExecutablePath}'.", ex);
        }

        Task<string> readOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> readError = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return new CommandResult(process.ExitCode, await readOutput, await readError);
    }

    private static string Quote(string value) => value.Contains(' ') ? $"\"{value}\"" : value;

    private static string ResolveAdbExecutablePath()
    {
        foreach (string projectRoot in EnumerateCandidateRoots())
        {
            string bundledAdbPath = Path.Combine(projectRoot, ".tools", "platform-tools", "adb.exe");
            if (File.Exists(bundledAdbPath))
            {
                return bundledAdbPath;
            }
        }

        return "adb";
    }

    private static IEnumerable<string> EnumerateCandidateRoots()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            yield return current.FullName;
            current = current.Parent;
        }
    }

    private static void EnsureSuccess(CommandResult result, string message)
    {
        if (result.ExitCode == 0)
        {
            return;
        }

        string detail = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
        throw new InvalidOperationException($"{message} adb exited with code {result.ExitCode}. {detail.Trim()}");
    }

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError);
}

internal sealed record WebViewSocketInfo(string SocketName, int ProcessId);

internal sealed class DevToolsDiscoveryClient
{
    private readonly HttpClient _httpClient;

    public DevToolsDiscoveryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DevToolsTargetInfo>> GetTargetsAsync(int port, CancellationToken cancellationToken)
    {
        Uri[] endpoints =
        [
            new($"http://127.0.0.1:{port}/json/list"),
            new($"http://127.0.0.1:{port}/json")
        ];

        foreach (var endpoint in endpoints)
        {
            try
            {
                var targets = await _httpClient.GetFromJsonAsync(endpoint, MonitorJsonContext.Default.ListDevToolsTargetInfo, cancellationToken);
                if (targets is { Count: > 0 })
                {
                    return targets;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (JsonException)
            {
            }
        }

        throw new InvalidOperationException("Failed to discover DevTools targets from the forwarded WebView endpoint.");
    }
}

internal sealed class WebViewTargetSelector
{
    private readonly AdbClient _adbClient;
    private readonly HttpClient _httpClient = new();

    public WebViewTargetSelector(AdbClient adbClient)
    {
        _adbClient = adbClient;
    }

    public async Task<TargetSelection> SelectAsync(MonitorOptions options, CancellationToken cancellationToken)
    {
        string deviceSerial = await _adbClient.GetSingleDeviceSerialAsync(cancellationToken);
        var sockets = await _adbClient.GetWebViewSocketsAsync(deviceSerial, cancellationToken);

        WebViewSocketInfo socket = options.SocketName is null
            ? sockets[0]
            : sockets.FirstOrDefault(socket => string.Equals(socket.SocketName, options.SocketName, StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"Requested socket '{options.SocketName}' was not found.");

        await _adbClient.RemoveForwardAsync(deviceSerial, options.Port, cancellationToken);
        await _adbClient.ForwardAsync(deviceSerial, options.Port, socket.SocketName, cancellationToken);

        var discoveryClient = new DevToolsDiscoveryClient(_httpClient);
        var targets = await discoveryClient.GetTargetsAsync(options.Port, cancellationToken);
        var target = targets.FirstOrDefault(static target => target.IsPageLike)
            ?? throw new InvalidOperationException("No page-like DevTools target was exposed by the selected WebView socket.");

        if (string.IsNullOrWhiteSpace(target.WebSocketDebuggerUrl))
        {
            throw new InvalidOperationException("The selected DevTools target does not expose a WebSocket debugger URL.");
        }

        return new TargetSelection(deviceSerial, socket.SocketName, options.Port, target);
    }
}

internal sealed record TargetSelection(string DeviceSerial, string SocketName, int Port, DevToolsTargetInfo Target);

internal sealed record DevToolsTargetInfo(
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("devtoolsFrontendUrl")] string? DevToolsFrontendUrl,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("webSocketDebuggerUrl")] string WebSocketDebuggerUrl)
{
    public bool IsPageLike => string.Equals(Type, "page", StringComparison.OrdinalIgnoreCase) || string.Equals(Type, "webview", StringComparison.OrdinalIgnoreCase);
}

internal sealed class NetworkMonitor
{
    private readonly AotCdpClient _protocolClient;
    private readonly JsonLogWriter _logWriter;
    private readonly ConcurrentDictionary<string, NetworkRequestRecord> _requests = [];

    public NetworkMonitor(AotCdpClient protocolClient, JsonLogWriter logWriter)
    {
        _protocolClient = protocolClient;
        _logWriter = logWriter;
    }

    public async Task RunAsync(MonitorOptions options, CancellationToken cancellationToken)
    {
        _options = options;
        await _protocolClient.ConnectAsync(cancellationToken);

        _protocolClient.EventReceived += OnRawEventReceived;
        try
        {
            await _protocolClient.EnableNetworkAsync(cancellationToken);
            await _protocolClient.EnablePageAsync(cancellationToken);
            await _protocolClient.EnableRuntimeAsync(cancellationToken);

            Console.WriteLine("[info] monitoring network events; press Ctrl+C to stop");
            if (options.ReloadAfterAttach)
            {
                Console.WriteLine("[info] reloading target after Network.enable (cache bypassed)");
                await _protocolClient.ReloadAsync(cancellationToken);
            }

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            _protocolClient.EventReceived -= OnRawEventReceived;
        }
    }

    // WebView's embedded CDP schema can lag behind the generated NuGet schema.
    // Read these common Network events from the library's raw event stream so a schema mismatch cannot drop them.
    private void OnRawEventReceived(object? sender, JsonObject @event)
    {
        try
        {
            string? method = GetString(@event, "method");
            JsonObject? parameters = @event["params"] as JsonObject;
            if (parameters is null)
            {
                return;
            }

            switch (method)
            {
                case "Network.requestWillBeSent":
                    OnRawRequestWillBeSent(parameters);
                    break;
                case "Network.responseReceived":
                    OnRawResponseReceived(parameters);
                    break;
                case "Network.loadingFailed":
                    OnRawLoadingFailed(parameters);
                    break;
                case "Network.loadingFinished":
                    _ = OnRawLoadingFinishedAsync(parameters);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[warn] unable to process CDP event: {ex.Message}");
        }
    }

    private void OnRawRequestWillBeSent(JsonObject parameters)
    {
        JsonObject? request = parameters["request"] as JsonObject;
        string? requestIdText = GetString(parameters, "requestId");
        string? url = request is null ? null : GetString(request, "url");
        if (string.IsNullOrWhiteSpace(requestIdText) || !ShouldTrack(_options, url))
        {
            return;
        }

        var record = _requests.GetOrAdd(requestIdText, static id => new NetworkRequestRecord(id));
        record.Url = url;
        record.Method = request is null ? null : GetString(request, "method");
        record.ResourceType = GetString(parameters, "type");
        record.RequestTimestamp = DateTimeOffset.Now;

        Console.WriteLine($"[{record.RequestTimestamp:HH:mm:ss}] --> {record.Method} {record.Url} ({record.ResourceType})");
        _ = _logWriter.WriteRequestAsync(new NetworkRequestLogEntry(
            "request",
            record.RequestTimestamp,
            requestIdText,
            record.Method,
            record.Url,
            record.ResourceType,
            GetString(parameters, "documentURL"),
            GetBoolean(request, "hasPostData"),
            GetString(request, "initialPriority")));
    }

    private void OnRawResponseReceived(JsonObject parameters)
    {
        JsonObject? response = parameters["response"] as JsonObject;
        string? requestIdText = GetString(parameters, "requestId");
        string? url = response is null ? null : GetString(response, "url");
        if (string.IsNullOrWhiteSpace(requestIdText) || !ShouldTrack(_options, url))
        {
            return;
        }

        var record = _requests.GetOrAdd(requestIdText, static id => new NetworkRequestRecord(id));
        record.Url ??= url;
        record.StatusCode = GetInt32(response, "status");
        record.StatusText = GetString(response, "statusText");
        record.MimeType = GetString(response, "mimeType");
        record.ResourceType ??= GetString(parameters, "type");
        record.ResponseTimestamp = DateTimeOffset.Now;

        Console.WriteLine($"[{record.ResponseTimestamp:HH:mm:ss}] <-- {record.StatusCode} {record.Url} ({record.MimeType})");
        _ = _logWriter.WriteResponseAsync(new NetworkResponseLogEntry(
            "response",
            record.ResponseTimestamp.Value,
            requestIdText,
            record.Url,
            record.StatusCode,
            record.StatusText,
            record.MimeType,
            record.ResourceType,
            GetBoolean(response, "fromDiskCache"),
            GetBoolean(response, "fromServiceWorker"),
            GetDouble(response, "encodedDataLength") ?? 0));
    }

    private void OnRawLoadingFailed(JsonObject parameters)
    {
        string? requestIdText = GetString(parameters, "requestId");
        NetworkRequestRecord? record = null;
        if (!string.IsNullOrWhiteSpace(requestIdText))
        {
            _requests.TryGetValue(requestIdText, out record);
        }

        if (record is not null && !ShouldTrack(_options, record.Url))
        {
            return;
        }

        DateTimeOffset timestamp = DateTimeOffset.Now;
        string? resourceType = GetString(parameters, "type");
        string errorText = GetString(parameters, "errorText") ?? "Unknown network error";
        Console.WriteLine($"[{timestamp:HH:mm:ss}] xx  {requestIdText} failed: {errorText}");
        _ = _logWriter.WriteFailureAsync(new NetworkFailureLogEntry(
            "failure",
            timestamp,
            requestIdText ?? string.Empty,
            record?.Url,
            resourceType,
            errorText,
            GetBoolean(parameters, "canceled")));
    }

    private Task OnRawLoadingFinishedAsync(JsonObject parameters)
    {
        string? requestIdText = GetString(parameters, "requestId");
        if (string.IsNullOrWhiteSpace(requestIdText))
        {
            return Task.CompletedTask;
        }

        return OnRawResponseBodyAsync(requestIdText);
    }

    private async Task OnRawResponseBodyAsync(string requestId)
    {
        if (!_requests.TryGetValue(requestId, out var record) || !ShouldCaptureBody(_options, record))
        {
            return;
        }

        try
        {
            JsonObject response = await _protocolClient.GetResponseBodyAsync(requestId, CancellationToken.None);
            string body = GetString(response, "body") ?? string.Empty;
            bool base64Encoded = GetBoolean(response, "base64Encoded") ?? false;
            if (base64Encoded)
            {
                await _logWriter.WriteBodyAsync(new NetworkBodyLogEntry("body", DateTimeOffset.Now, requestId, record.Url, record.MimeType, true, body.Length, null, null));
                return;
            }

            string preview = Truncate(body, _options.BodyPreviewMaxBytes);
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] body {record.Url}");
            Console.WriteLine(preview);
            await _logWriter.WriteBodyAsync(new NetworkBodyLogEntry("body", DateTimeOffset.Now, requestId, record.Url, record.MimeType, false, Encoding.UTF8.GetByteCount(body), preview, body.Length > preview.Length));
        }
        catch (Exception ex)
        {
            await _logWriter.WriteBodyAsync(new NetworkBodyLogEntry("body_error", DateTimeOffset.Now, requestId, record.Url, record.MimeType, false, null, null, null, ex.Message));
        }
    }

    private static string? GetString(JsonObject? value, string propertyName) =>
        value?[propertyName]?.GetValue<string>();

    private static bool? GetBoolean(JsonObject? value, string propertyName) =>
        value?[propertyName] is JsonValue jsonValue && jsonValue.TryGetValue<bool>(out bool result) ? result : null;

    private static int? GetInt32(JsonObject? value, string propertyName) =>
        value?[propertyName] is JsonValue jsonValue && jsonValue.TryGetValue<int>(out int result) ? result : null;

    private static double? GetDouble(JsonObject? value, string propertyName) =>
        value?[propertyName] is JsonValue jsonValue && jsonValue.TryGetValue<double>(out double result) ? result : null;

    #if false
    private void OnRequestWillBeSent(MonitorOptions options, Network.RequestWillBeSent @event)
    {
        if (!ShouldTrack(options, @event.Request.Url))
        {
            return;
        }

        var record = _requests.GetOrAdd(@event.RequestId, static requestId => new NetworkRequestRecord(requestId));
        record.Url = @event.Request.Url;
        record.Method = @event.Request.Method;
        record.ResourceType = @event.Type?.ToString();
        record.RequestTimestamp = DateTimeOffset.Now;

        Console.WriteLine($"[{record.RequestTimestamp:HH:mm:ss}] --> {record.Method} {record.Url} ({record.ResourceType})");
        _ = _logWriter.WriteRequestAsync(new NetworkRequestLogEntry(
            "request",
            record.RequestTimestamp,
            @event.RequestId.ToString(),
            record.Method,
            record.Url,
            record.ResourceType,
            @event.DocumentURL,
            @event.Request.HasPostData,
            @event.Request.InitialPriority.ToString()));
    }

    private void OnResponseReceived(MonitorOptions options, Network.ResponseReceived @event)
    {
        if (!ShouldTrack(options, @event.Response.Url))
        {
            return;
        }

        var record = _requests.GetOrAdd(@event.RequestId, static requestId => new NetworkRequestRecord(requestId));
        record.Url ??= @event.Response.Url;
        record.StatusCode = (int) @event.Response.Status;
        record.StatusText = @event.Response.StatusText;
        record.MimeType = @event.Response.MimeType;
        record.ResourceType ??= @event.Type?.ToString();
        record.ResponseTimestamp = DateTimeOffset.Now;

        Console.WriteLine($"[{record.ResponseTimestamp:HH:mm:ss}] <-- {record.StatusCode} {record.Url} ({record.MimeType})");
        _ = _logWriter.WriteResponseAsync(new NetworkResponseLogEntry(
            "response",
            record.ResponseTimestamp.Value,
            @event.RequestId.ToString(),
            record.Url,
            record.StatusCode,
            record.StatusText,
            record.MimeType,
            record.ResourceType,
            @event.Response.FromDiskCache,
            @event.Response.FromServiceWorker,
            @event.Response.EncodedDataLength));
    }

    private void OnLoadingFailed(MonitorOptions options, Network.LoadingFailed @event)
    {
        NetworkRequestRecord? record = null;
        if (_requests.TryGetValue(@event.RequestId, out record) && !ShouldTrack(options, record.Url))
        {
            return;
        }

        DateTimeOffset timestamp = DateTimeOffset.Now;
        Console.WriteLine($"[{timestamp:HH:mm:ss}] xx  {@event.RequestId} failed: {@event.ErrorText}");
        _ = _logWriter.WriteFailureAsync(new NetworkFailureLogEntry(
            "failure",
            timestamp,
            @event.RequestId.ToString(),
            record?.Url,
            @event.Type.ToString(),
            @event.ErrorText,
            @event.Canceled));
    }

    private async Task OnLoadingFinishedAsync(Network.LoadingFinished @event)
    {
        if (!_requests.TryGetValue(@event.RequestId, out var record))
        {
            return;
        }

        if (!ShouldCaptureBody(_options, record))
        {
            return;
        }

        try
        {
            var response = await _protocolClient.SendCommandAsync(Network.GetResponseBody(@event.RequestId), null, CancellationToken.None);

            if (response.Base64Encoded)
            {
                Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] body {record.Url} [base64 payload, length={response.Body.Length}]");
                await _logWriter.WriteBodyAsync(new NetworkBodyLogEntry(
                    "body",
                    DateTimeOffset.Now,
                    @event.RequestId.ToString(),
                    record.Url,
                    record.MimeType,
                    true,
                    response.Body.Length,
                    null,
                    null));
                return;
            }

            string preview = Truncate(response.Body, _options.BodyPreviewMaxBytes);
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] body {record.Url}");
            Console.WriteLine(preview);
            await _logWriter.WriteBodyAsync(new NetworkBodyLogEntry(
                "body",
                DateTimeOffset.Now,
                @event.RequestId.ToString(),
                record.Url,
                record.MimeType,
                false,
                Encoding.UTF8.GetByteCount(response.Body),
                preview,
                response.Body.Length > preview.Length));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] body {record.Url} [unavailable: {ex.Message}]");
            await _logWriter.WriteBodyAsync(new NetworkBodyLogEntry(
                "body_error",
                DateTimeOffset.Now,
                @event.RequestId.ToString(),
                record.Url,
                record.MimeType,
                false,
                null,
                null,
                null,
                ex.Message));
        }
    }
    #endif

    private MonitorOptions _options = new(null, 9222, null, false, false, 16 * 1024, string.Empty, false);

    private static bool ShouldTrack(MonitorOptions options, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(options.UrlMatchKeyword)
            || url.Contains(options.UrlMatchKeyword, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldCaptureBody(MonitorOptions options, NetworkRequestRecord record)
    {
        if (!options.CaptureBody || string.IsNullOrWhiteSpace(record.Url))
        {
            return false;
        }

        return record.ResourceType is "XHR" or "Fetch" or "Document";
    }

    private static string Truncate(string text, int maxBytes)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        if (bytes.Length <= maxBytes)
        {
            return text;
        }

        string preview = Encoding.UTF8.GetString(bytes, 0, maxBytes);
        return $"{preview}{Environment.NewLine}[truncated to {maxBytes} bytes]";
    }
}

internal sealed class NetworkRequestRecord
{
    public NetworkRequestRecord(string requestId)
    {
        RequestId = requestId;
    }

    public string RequestId { get; }
    public string? Url { get; set; }
    public string? Method { get; set; }
    public string? ResourceType { get; set; }
    public int? StatusCode { get; set; }
    public string? StatusText { get; set; }
    public string? MimeType { get; set; }
    public DateTimeOffset RequestTimestamp { get; set; }
    public DateTimeOffset? ResponseTimestamp { get; set; }
}

internal sealed class AotCdpClient : IDisposable
{
    private readonly ClientWebSocket _socket = new();
    private readonly Uri _endpoint;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonObject>> _responses = [];
    private int _nextId;

    public AotCdpClient(Uri endpoint) => _endpoint = endpoint;

    public event EventHandler<JsonObject>? EventReceived;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _socket.ConnectAsync(_endpoint, cancellationToken);
        _ = Task.Run(ReceiveLoopAsync);
    }

    public Task EnableNetworkAsync(CancellationToken token) => SendAsync("Network.enable", writer =>
    {
        writer.WriteNumber("maxTotalBufferSize", 8 * 1024 * 1024);
        writer.WriteNumber("maxResourceBufferSize", 1024 * 1024);
        writer.WriteNumber("maxPostDataSize", 1024 * 1024);
    }, token);

    public Task EnablePageAsync(CancellationToken token) => SendAsync("Page.enable", null, token);
    public Task EnableRuntimeAsync(CancellationToken token) => SendAsync("Runtime.enable", null, token);
    public Task ReloadAsync(CancellationToken token) => SendAsync("Page.reload", writer => writer.WriteBoolean("ignoreCache", true), token);

    public Task<JsonObject> GetResponseBodyAsync(string requestId, CancellationToken token) =>
        SendAsync("Network.getResponseBody", writer => writer.WriteString("requestId", requestId), token);

    private async Task<JsonObject> SendAsync(string method, Action<Utf8JsonWriter>? writeParameters, CancellationToken token)
    {
        int id = Interlocked.Increment(ref _nextId);
        var completion = new TaskCompletionSource<JsonObject>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_responses.TryAdd(id, completion)) throw new InvalidOperationException("Unable to register CDP command.");

        try
        {
            var bytes = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bytes))
            {
                writer.WriteStartObject();
                writer.WriteNumber("id", id);
                writer.WriteString("method", method);
                if (writeParameters is not null)
                {
                    writer.WritePropertyName("params");
                    writer.WriteStartObject();
                    writeParameters(writer);
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }

            await _socket.SendAsync(bytes.WrittenMemory, WebSocketMessageType.Text, true, token);
            using var registration = token.Register(() => completion.TrySetCanceled(token));
            return await completion.Task;
        }
        finally
        {
            _responses.TryRemove(id, out _);
        }
    }

    private async Task ReceiveLoopAsync()
    {
        byte[] buffer = new byte[16 * 1024];
        var bytes = new ArrayBufferWriter<byte>();
        try
        {
            while (_socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close) return;
                    bytes.Write(buffer.AsSpan(0, result.Count));
                } while (!result.EndOfMessage);

                JsonObject? message = JsonNode.Parse(bytes.WrittenSpan) as JsonObject;
                bytes = new ArrayBufferWriter<byte>();
                if (message is null) continue;
                if (message["id"] is JsonValue idValue && idValue.TryGetValue<int>(out int id))
                {
                    if (_responses.TryGetValue(id, out var completion))
                    {
                        if (message["error"] is not null) completion.TrySetException(new InvalidOperationException(message["error"]!.ToJsonString()));
                        else completion.TrySetResult(message["result"] as JsonObject ?? new JsonObject());
                    }
                    continue;
                }

                EventReceived?.Invoke(this, message);
            }
        }
        catch (Exception ex)
        {
            foreach (var response in _responses.Values) response.TrySetException(ex);
        }
    }

    public void Dispose()
    {
        _socket.Dispose();
    }
}

[JsonSerializable(typeof(List<DevToolsTargetInfo>))]
[JsonSerializable(typeof(MonitorSessionStartedLogEntry))]
[JsonSerializable(typeof(NetworkRequestLogEntry))]
[JsonSerializable(typeof(NetworkResponseLogEntry))]
[JsonSerializable(typeof(NetworkFailureLogEntry))]
[JsonSerializable(typeof(NetworkBodyLogEntry))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class MonitorJsonContext : JsonSerializerContext;

internal sealed class JsonLogWriter : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonLogWriter(string path)
    {
        string fullPath = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(fullPath, append: true, Encoding.UTF8);
    }

    public Task WriteSessionAsync(MonitorSessionStartedLogEntry entry) => WriteLineAsync(JsonSerializer.Serialize(entry, MonitorJsonContext.Default.MonitorSessionStartedLogEntry));
    public Task WriteRequestAsync(NetworkRequestLogEntry entry) => WriteLineAsync(JsonSerializer.Serialize(entry, MonitorJsonContext.Default.NetworkRequestLogEntry));
    public Task WriteResponseAsync(NetworkResponseLogEntry entry) => WriteLineAsync(JsonSerializer.Serialize(entry, MonitorJsonContext.Default.NetworkResponseLogEntry));
    public Task WriteFailureAsync(NetworkFailureLogEntry entry) => WriteLineAsync(JsonSerializer.Serialize(entry, MonitorJsonContext.Default.NetworkFailureLogEntry));
    public Task WriteBodyAsync(NetworkBodyLogEntry entry) => WriteLineAsync(JsonSerializer.Serialize(entry, MonitorJsonContext.Default.NetworkBodyLogEntry));

    private async Task WriteLineAsync(string line)
    {
        await _gate.WaitAsync();
        try
        {
            await _writer.WriteLineAsync(line);
            await _writer.FlushAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _writer.Dispose();
        _gate.Dispose();
    }
}

internal sealed record MonitorSessionStartedLogEntry(
    string EventType,
    DateTimeOffset Timestamp,
    string DeviceSerial,
    string SocketName,
    int Port,
    string TargetTitle,
    string TargetUrl,
    string? UrlMatchKeyword,
    bool CaptureBody,
    bool ReloadAfterAttach,
    int BodyPreviewMaxBytes);

internal sealed record NetworkRequestLogEntry(
    string EventType,
    DateTimeOffset Timestamp,
    string RequestId,
    string? Method,
    string? Url,
    string? ResourceType,
    string? DocumentUrl,
    bool? HasPostData,
    string? Priority);

internal sealed record NetworkResponseLogEntry(
    string EventType,
    DateTimeOffset Timestamp,
    string RequestId,
    string? Url,
    int? StatusCode,
    string? StatusText,
    string? MimeType,
    string? ResourceType,
    bool? FromDiskCache,
    bool? FromServiceWorker,
    double EncodedDataLength);

internal sealed record NetworkFailureLogEntry(
    string EventType,
    DateTimeOffset Timestamp,
    string RequestId,
    string? Url,
    string? ResourceType,
    string ErrorText,
    bool? Canceled);

internal sealed record NetworkBodyLogEntry(
    string EventType,
    DateTimeOffset Timestamp,
    string RequestId,
    string? Url,
    string? MimeType,
    bool Base64Encoded,
    int? ByteLength,
    string? Preview,
    bool? Truncated,
    string? Error = null);
