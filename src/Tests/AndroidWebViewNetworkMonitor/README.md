# Android WebView Network Monitor

This sample connects to a debuggable Android WebView over `adb`, forwards the DevTools socket to a local port, and monitors network traffic through Chrome DevTools Protocol.

## Requirements

1. The sample includes a local `adb` under `.tools/platform-tools`, or you have `adb` on `PATH`.
2. Exactly one Android device is connected over USB and visible in `adb devices`.
3. The target app enables `WebView.setWebContentsDebuggingEnabled(true)`.

## Usage

```powershell
dotnet run --project src/Tests/AndroidWebViewNetworkMonitor -- --body --reload --match api
```

The sample prefers `src/Tests/AndroidWebViewNetworkMonitor/.tools/platform-tools/adb.exe` and only falls back to `adb` on `PATH` when the bundled copy is missing.
By default it also writes JSONL logs to `bin/<Configuration>/net10.0/logs/monitor-<timestamp>.jsonl`.

Options:

- `--socket <name>` chooses a specific `webview_devtools_remote*` socket.
- `--port <number>` changes the forwarded local port. Default: `9222`.
- `--match <keyword>` filters requests by URL substring.
- `--body` enables response body capture for `XHR`, `Fetch`, and `Document`.
- `--reload` reloads the target only after CDP monitoring is active, bypassing cache. Use it to capture requests that already completed before the monitor attached.
- `--body-preview-kb <n>` limits text response preview size. Default: `16`.
- `--log-file <path>` writes logs to a specific `.jsonl` file.

## Native AOT publishing

```powershell
dotnet publish src/Tests/AndroidWebViewNetworkMonitor -c Release -r win-x64
```

The monitor uses an AOT-safe CDP transport based on `ClientWebSocket` and source-generated JSON logging. `ChromeProtocol.Runtime` is not used at runtime because version 2.1.1 discovers protocol types through reflection, which is incompatible with Native AOT.
