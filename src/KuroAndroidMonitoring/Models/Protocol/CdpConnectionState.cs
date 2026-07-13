namespace ChromeCDPSharp.Models;

public enum CdpConnectionState
{
    None,
    Connecting,
    Connected,
    Disconnecting,
    Disconnected,
    Faulted,
    Disposed
}
