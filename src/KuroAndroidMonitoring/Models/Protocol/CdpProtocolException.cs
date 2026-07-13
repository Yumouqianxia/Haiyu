using System.Text.Json;

namespace ChromeCDPSharp.Models;

public sealed class CdpProtocolException : InvalidOperationException
{
    public CdpProtocolException(string method, CdpErrorObject error)
        : base(BuildMessage(method, error))
    {
        Method = method;
        ErrorCode = error.Code;
        ErrorData = error.Data.Clone();
    }

    public string Method { get; }

    public int ErrorCode { get; }

    public JsonElement ErrorData { get; }

    private static string BuildMessage(string method, CdpErrorObject error)
    {
        return $"CDP command '{method}' failed with code {error.Code}: {error.Message}";
    }
}
