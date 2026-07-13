using System.Net.WebSockets;

namespace ChromeCDPSharp.Models;

public sealed class CdpConnectionStateChangedEventArgs : EventArgs
{
    public CdpConnectionStateChangedEventArgs(
        CdpConnectionState previousState,
        CdpConnectionState currentState,
        WebSocketState webSocketState,
        Exception? exception = null,
        string? message = null)
    {
        PreviousState = previousState;
        CurrentState = currentState;
        WebSocketState = webSocketState;
        Exception = exception;
        Message = message;
    }

    public CdpConnectionState PreviousState { get; }

    public CdpConnectionState CurrentState { get; }

    public WebSocketState WebSocketState { get; }

    public Exception? Exception { get; }

    public string? Message { get; }
}
