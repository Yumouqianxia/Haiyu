using System.Net;

namespace ProxyExtensions;

/// <summary>
/// GitHub 代理
/// </summary>
public class GithubProxy : IWebProxy
{
    private readonly WebProxy? _inner;
    private readonly HashSet<string> _gitHubHosts;
    public ICredentials? Credentials { get; set;  }

    public GithubProxy(string? proxyUrl, Dictionary<string, IPAddress[]> gitHubHosts)
    {
        _gitHubHosts = new HashSet<string>(gitHubHosts.Keys, StringComparer.OrdinalIgnoreCase);
        _gitHubHosts.Add("gist.github.com");

        if (!string.IsNullOrWhiteSpace(proxyUrl))
        {
            if (!proxyUrl.Contains("://", StringComparison.Ordinal))
                proxyUrl = "http://" + proxyUrl;

            if (Uri.TryCreate(proxyUrl, UriKind.Absolute, out var uri))
                _inner = new WebProxy(uri);
        }
    }

    public Uri? GetProxy(Uri destination)
    {
        if (_inner is not null)
            return _inner.GetProxy(destination);

        return destination;
    }

    public bool IsBypassed(Uri host)
    {
        if (IsGitHubHost(host.Host))
            return true;

        if (_inner is not null)
            return _inner.IsBypassed(host);

        return true;
    }

    private bool IsGitHubHost(string host)
    {
        if (_gitHubHosts.Contains(host))
            return true;

        if (host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
