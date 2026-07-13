using ChromeCDPSharp.Common;
using ChromeCDPSharp.Models;
using ChromeCDPSharp.Serialization;

namespace Project.Test;

[TestClass]
public class AdbTest
{
    [TestMethod]
    public async Task TestMethod1()
    {
        AdbClient adbClient = new AdbClient();
        adbClient.InitAdbServer(@"E:\MuMu Player 12\shell\adb.exe");
        var devices = await adbClient.GetDevicesAsync();
        var device = devices[0];
        var sockets = await adbClient.GetWebViewSocketsAsync(device.Serial);
        var socket = sockets[0];
        string webSocketDebuggerUrl = await adbClient.GetWebSocketDebuggerUrlAsync(device.Serial, socket.SocketName, 9085);
        Console.WriteLine(webSocketDebuggerUrl);

        await using CDPClient cdpClient = new(webSocketDebuggerUrl);
        await cdpClient.ConnectAsync();
        Console.WriteLine($"CDP socket state: {cdpClient.State}");

        await cdpClient.SendCommandAsync(
            "Network.enable",
            new NetworkEnableParams(
                MaxTotalBufferSize: 8 * 1024 * 1024,
                MaxResourceBufferSize: 1024 * 1024,
                MaxPostDataSize: 1024 * 1024),
            CdpJsonContext.Default.NetworkEnableParams,
            CdpJsonContext.Default.CdpCommandResponseEmptyResult);

        using IDisposable requestSubscription = cdpClient.Subscribe(
            "Network.requestWillBeSent",
            CdpJsonContext.Default.RequestWillBeSentEvent,
            static request =>
            {
                Console.WriteLine($"REQ {request.RequestId} {request.Request.Method} {request.Request.Url}");
                return ValueTask.CompletedTask;
            });

        using IDisposable responseSubscription = cdpClient.Subscribe(
            "Network.responseReceived",
            CdpJsonContext.Default.ResponseReceivedEvent,
            static response =>
            {
                Console.WriteLine($"RES {response.RequestId} {response.Response.Status} {response.Response.Url}");
                return ValueTask.CompletedTask;
            });

        using IDisposable failedSubscription = cdpClient.Subscribe(
            "Network.loadingFailed",
            CdpJsonContext.Default.LoadingFailedEvent,
            static failed =>
            {
                Console.WriteLine($"FAIL {failed.RequestId} {failed.ErrorText}");
                return ValueTask.CompletedTask;
            });

        await Task.Delay(TimeSpan.FromSeconds(60));
    }
}
