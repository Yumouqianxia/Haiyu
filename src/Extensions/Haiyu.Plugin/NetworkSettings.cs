using Haiyu.Plugin.Models;

namespace Haiyu.Plugin;

/// <summary>
/// 网络配置
/// </summary>
public class NetworkSettings
{
    public NetworkSettings(string baseJsonSettingPath)
    {
        this.LocalFile = baseJsonSettingPath;
    }

    public string LocalFile { get; }

    public async Task<ProxyItem> GetLocalGithubFillterAsync()
    {
        return new();
    }
}
