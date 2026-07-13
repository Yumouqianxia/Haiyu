using ChromeCDPSharp.Common;

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
        var socket = await adbClient.GetWebViewSocketsAsync(device.Serial);
        
    }
}
