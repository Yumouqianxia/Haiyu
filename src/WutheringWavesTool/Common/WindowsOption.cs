namespace Haiyu.Common;

public sealed record WindowsOption
{
    public double? Width { get; init; }
    public double? Height { get; init; }
    public double? MinWidth { get; init; }
    public double? MinHeight { get; init; }
    public double? MaxWidth { get; init; }
    public double? MaxHeight { get; init; }
    public bool? IsResizable { get; init; }
    public bool? IsMaximizable { get; init; }
    public bool? IsMinimizable { get; init; }
    public bool CenterOnScreen { get; init; }
}
