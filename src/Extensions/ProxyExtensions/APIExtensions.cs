using System.Net;

namespace ProxyExtensions;

public static class APIExtensions
{
    private const string GitHubUserContentHost = "*.githubusercontent.com";

    private static readonly Dictionary<string, string[]> GitHubHostAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["release-assets.githubusercontent.com"] =
            [
                "release-assets.githubusercontent.com",
                "objects.githubusercontent.com",
                "raw.githubusercontent.com",
            ],
            ["objects.githubusercontent.com"] =
            [
                "objects.githubusercontent.com",
                "release-assets.githubusercontent.com",
                "raw.githubusercontent.com",
            ],
            ["objects-origin.githubusercontent.com"] =
            [
                "objects-origin.githubusercontent.com",
                "github.com",
            ],
            ["github-releases.githubusercontent.com"] =
            [
                "github-releases.githubusercontent.com",
                "github-registry-files.githubusercontent.com",
            ],
            ["github-registry-files.githubusercontent.com"] =
            [
                "github-registry-files.githubusercontent.com",
                "github-releases.githubusercontent.com",
            ],
        };

    extension(string host)
    {
        public bool FilterAddress(Dictionary<string, IPAddress[]> query,out IReadOnlyList<IPAddress> point)
        {
            if (query.TryGetValue(host, out var list))
            {
                point = list;
                return true;
            }

            if (GitHubHostAliases.TryGetValue(host, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    if (query.TryGetValue(alias, out list))
                    {
                        point = list;
                        return true;
                    }
                }
            }

            if (host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase))
            {
                if (query.TryGetValue(GitHubUserContentHost, out list))
                {
                    point = list;
                    return true;
                }

                if (query.TryGetValue("raw.githubusercontent.com", out list))
                {
                    point = list;
                    return true;
                }
            }

            point = Array.Empty<IPAddress>();
            return false;
        }
    }
}
