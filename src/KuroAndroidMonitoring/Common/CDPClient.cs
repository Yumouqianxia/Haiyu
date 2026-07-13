namespace ChromeCDPSharp.Common;

public class CDPClient
{
    public CDPClient(string webSocketDebugUrl)
    {
        this.DebugUrl = webSocketDebugUrl;
    }

    public string DebugUrl { get; }
}
