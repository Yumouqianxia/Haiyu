using System.Net;

namespace ProxyExtensions;

public static class APIExtensions
{
    extension(string host)
    {
        public bool FilterAddress(Dictionary<string, IPAddress[]> query,out IReadOnlyList<IPAddress> point)
        {
            if (query.TryGetValue(host, out var list))
            {
                point = list;
                return true;
            }

            if (host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase) &&
                query.TryGetValue("raw.githubusercontent.com", out list))
            {
                point = list;
                return true;
            }

            point = Array.Empty<IPAddress>();
            return false;
        }
    }
}
