namespace Waves.Core.Common;

public static class NetworkCheck
{
    public static async Task<PingReply?> PingAsync(string host)
    {
        try
        {
            var uri = new Uri(host);
            Ping ping = new();
            return await ping.SendPingAsync(uri.Host);
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}