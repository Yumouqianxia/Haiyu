using System.Text.Json.Serialization;

namespace ChromeCDPSharp.Models;

public sealed record DevToolsTargetInfo(
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("devtoolsFrontendUrl")] string? DevToolsFrontendUrl,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("webSocketDebuggerUrl")] string WebSocketDebuggerUrl)
{
    public bool IsPageLike =>
        string.Equals(Type, "page", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Type, "webview", StringComparison.OrdinalIgnoreCase);
}
