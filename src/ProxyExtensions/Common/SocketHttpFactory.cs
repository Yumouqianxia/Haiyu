using System.Net;
using System.Net.Sockets;

namespace ProxyExtensions.Common;

public static class SocketHttpFactory
{
    public static SocketsHttpHandler CreateGithubHandler(
        string proxy,
        Dictionary<string, IPAddress[]> ipEndpoint
    )
    {
        var handler = new SocketsHttpHandler()
        {
            UseProxy = true,
            Proxy = new GithubProxy(proxy, ipEndpoint),
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(20),
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            ConnectCallback = async (context, token) =>
            {
                var host = context.DnsEndPoint.Host;
                var port = context.DnsEndPoint.Port;
                if (host.FilterAddress(ipEndpoint, out var addresses))
                {
                    var failures = new List<Exception>();

                    foreach (var address in addresses.Distinct())
                    {
                        token.ThrowIfCancellationRequested();

                        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
                        timeout.CancelAfter(TimeSpan.FromSeconds(5));

                        var socket = new Socket(
                            address.AddressFamily,
                            SocketType.Stream,
                            ProtocolType.Tcp
                        )
                        {
                            NoDelay = true,
                        };

                        try
                        {
                            await socket.ConnectAsync(new IPEndPoint(address, port), timeout.Token);
                            return new NetworkStream(socket, ownsSocket: true);
                        }
                        catch (OperationCanceledException) when (!token.IsCancellationRequested)
                        {
                            socket.Dispose();
                            failures.Add(
                                new TimeoutException($"Timed out: {host}:{port} via {address}")
                            );
                        }
                        catch (Exception ex) when (ex is SocketException or IOException)
                        {
                            socket.Dispose();
                            failures.Add(
                                new IOException($"Failed: {host}:{port} via {address}", ex)
                            );
                        }
                    }

                    throw new IOException(
                        $"Could not connect to {host}:{port}. Tried: {string.Join(", ", addresses)}",
                        new AggregateException(failures)
                    );
                }
                else
                {
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                    try
                    {
                        await socket.ConnectAsync(context.DnsEndPoint, token);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            },
        };

        return handler;
    }

    public static HttpClient CreateGithubClient(
        string proxy,
        Dictionary<string, IPAddress[]> address
    )
    {
        var client = new HttpClient(CreateGithubHandler(proxy, address), true)
        {
            Timeout = TimeSpan.FromSeconds(60),
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };
        return client;
    }
}
