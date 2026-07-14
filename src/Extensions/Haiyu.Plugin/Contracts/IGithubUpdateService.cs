using System.Net;

namespace Haiyu.Plugin.Contracts;

public interface IGithubUpdateService
{
    public void SetIps(Dictionary<string, IPAddress[]> ips);
}
