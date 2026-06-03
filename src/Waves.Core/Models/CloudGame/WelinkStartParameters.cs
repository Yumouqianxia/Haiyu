namespace Waves.Core.Models.CloudGame;

public sealed class WelinkStartParameters(
    string TenantKey,
    string GameId,
    string Resolution,
    int BitRate,
    int Fps,
    int CodecType,
    string Version,
    string CmdLine,
    string BizData,
    IEnumerable<CloudGameNode> nodes,
    CloudGameNode node
) : ICloneable
{
    public string TenantKey { get; } = TenantKey;
    public string GameId { get; } = GameId;
    public string Resolution { get; } = Resolution;
    public int BitRate { get; } = BitRate;
    public int Fps { get; } = Fps;
    public int CodecType { get; } = CodecType;
    public string Version { get; } = Version;
    public string CmdLine { get; } = CmdLine;
    public string BizData { get; } = BizData;
    public IEnumerable<CloudGameNode> Nodes { get; } = nodes;
    public CloudGameNode Node { get; } = node;

    public object Clone()
    {
        return new WelinkStartParameters(
            TenantKey,
            GameId,
            Resolution,
            BitRate,
            Fps,
            CodecType,
            Version,
            CmdLine,
            BizData,
            Nodes,
            Node
        );
    }
}