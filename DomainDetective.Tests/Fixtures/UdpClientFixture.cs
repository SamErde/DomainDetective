using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective.Tests.Fixtures;

public sealed class UdpClientFixture : IAsyncLifetime
{
    private readonly Func<UdpClient, CancellationToken, Task> _server;
    private readonly UdpClient _client;
    private readonly CancellationTokenSource _cts = new();
    private Task? _serverTask;

    public IPEndPoint LocalEndpoint { get; }

    public UdpClientFixture(Func<UdpClient, CancellationToken, Task> server)
    {
        _server = server;
        _client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        LocalEndpoint = (IPEndPoint)_client.Client.LocalEndPoint!;
    }

    public Task InitializeAsync()
    {
        _serverTask = _server(_client, _cts.Token);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _cts.Cancel();
        _client.Dispose();
        if (_serverTask != null)
        {
            try { await _serverTask; } catch { }
        }

        _cts.Dispose();
    }
}
