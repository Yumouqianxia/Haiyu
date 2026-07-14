using Haiyu.Common;
using Haiyu.Plugin.Common;
using Haiyu.Plugin.Contracts;
using ProxyExtensions.Models;
using System.Net;

namespace Haiyu.ViewModel;

partial class SettingViewModel
{
    [ObservableProperty]
    public partial List<string> UpdateAppType { get; set; } = ["Github", "Mirror"];

    [ObservableProperty]
    public partial ObservableCollection<GithubIpDisplayGroup> GithubIpGroups { get; set; } = [];

    [ObservableProperty]
    public partial bool HasGithubIpGroups { get; set; }

    [ObservableProperty]
    public partial string SelectUpdateAppType { get; set; }

    async partial void OnSelectUpdateAppTypeChanged(string value)
    {
        if (await AppSettings.GetUpdateTypeAsync() != value)
        {
            await AppSettings.SetUpdateTypeAsync(value);
        }
    }

    [ObservableProperty]
    public partial string? MirrorKey { get; set; }

    public async Task LoadUpdateAppType()
    {
        if (DesktopBridge.IsRunningAsMsix())
        {
            CheckUpdateVisibility = false;
            return;
        }

        CheckUpdateVisibility = true;
        var updateType = await AppSettings.GetUpdateTypeAsync();
        foreach (var item in UpdateAppType.Index())
        {
            if (updateType == item.Item)
            {
                SelectUpdateAppType = item.Item;
            }
        }

        if (SelectUpdateAppType == null)
        {
            SelectUpdateAppType = UpdateAppType[0];
        }

        MirrorKey = await AppSettings.GetMirrorKeyAsync();
        await LoadGithubIpConfigAsync();
    }

    private async Task LoadGithubIpConfigAsync()
    {
        var settings = await GithubIpSettings.GetMergedGithubIpsAsync();

        GithubIpGroups = settings
            .Where(x => !string.IsNullOrWhiteSpace(x.Host))
            .Select(CreateGithubIpDisplayGroup)
            .ToObservableCollection();

        HasGithubIpGroups = GithubIpGroups.Count > 0;
    }

    private static GithubIpDisplayGroup CreateGithubIpDisplayGroup(IPEndPointWrapper setting)
    {
        return new GithubIpDisplayGroup
        {
            Host = setting.Host,
            IpEditorText = string.Join(
                ", ",
                setting.Ips.Where(x => !string.IsNullOrWhiteSpace(x))
            ),
        };
    }

    [RelayCommand]
    async Task SaveGithubIpConfig()
    {
        List<IPEndPointWrapper> settings = [];
        foreach (var group in GithubIpGroups.Where(x => !string.IsNullOrWhiteSpace(x.Host)))
        {
            var ips = group
                .IpEditorText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ips.Count == 0)
            {
                continue;
            }

            var invalidIp = ips.FirstOrDefault(ip => !IPAddress.TryParse(ip, out _));
            if (invalidIp != null)
            {
                TipShow.ShowMessage($"{group.Host} 存在无效 IP：{invalidIp}", Symbol.Important);
                return;
            }

            settings.Add(
                new IPEndPointWrapper
                {
                    Host = group.Host.Trim(),
                    Ips = ips,
                }
            );
        }

        await GithubIpSettings.SetgithubIpsAsync(settings);
        await LoadGithubIpConfigAsync();
        TipShow.ShowMessage("Github 域前置配置已保存", Symbol.Accept);
    }

    [RelayCommand]
    async Task UpdateVersion()
    {
        await AppContext.UpdateAppAsync(true);
    }

    [RelayCommand]
    async Task SetMirrorKey()
    {
        await AppSettings.SetMirrorKeyAsync(MirrorKey);
        if (
            Instance.Host.Services.GetRequiredKeyedService<IUpdateService>("Mirror")
            is IMirrorUpdateService mirror
        )
        {
            mirror.SetMirrorKey(MirrorKey);
        }

        TipShow.ShowMessage("设置成功！", Symbol.Accept);
    }
}
