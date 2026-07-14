namespace Haiyu.ViewModel;

public sealed partial class GithubIpDisplayGroup : ObservableObject
{
    [ObservableProperty]
    public partial string Host { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IpEditorText { get; set; } = string.Empty;
}
