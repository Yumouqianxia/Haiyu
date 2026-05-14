namespace Waves.Core.Models.CloudGame;


public sealed record RuntimeSessionSnapshot(string SessionKey, int TotalTime, bool HasMessages);

public sealed record PingNode(string NodeId, int Delay);
