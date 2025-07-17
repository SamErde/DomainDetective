using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective;

/// <summary>
/// Performs a basic SNMP check against a server.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public class SnmpAnalysis
{
    /// <summary>SNMP query results keyed by host and port.</summary>
    public Dictionary<string, bool> ServerResults { get; private set; } = new();

    /// <summary>Maximum wait time for each query.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    internal Func<string, int, Task<bool>>? SnmpTestOverride { get; set; }

    /// <summary>Tests a single server for SNMP responses.</summary>
    public async Task AnalyzeServer(string host, int port, InternalLogger logger, CancellationToken cancellationToken = default)
    {
        ServerResults.Clear();
        var result = await CheckSnmpAsync(host, port, logger, cancellationToken);
        ServerResults[$"{host}:{port}"] = result;
    }

    /// <summary>Tests multiple servers for SNMP responses.</summary>
    public async Task AnalyzeServers(IEnumerable<string> hosts, IEnumerable<int> ports, InternalLogger logger, CancellationToken cancellationToken = default)
    {
        ServerResults.Clear();
        foreach (var host in hosts)
        {
            foreach (var port in ports)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await CheckSnmpAsync(host, port, logger, cancellationToken);
                ServerResults[$"{host}:{port}"] = result;
            }
        }
    }

    private static readonly byte[] _probe = new byte[]
    {
        0x30,0x26,0x02,0x01,0x00,0x04,0x06,0x70,0x75,0x62,0x6c,0x69,0x63,0xa0,0x19,0x02,0x04,0x00,0x00,0x00,0x01,0x02,0x01,0x00,0x02,0x01,0x00,0x30,0x0b,0x30,0x09,0x06,0x05,0x2b,0x06,0x01,0x02,0x01,0x05,0x00
    };

    private async Task<bool> CheckSnmpAsync(string host, int port, InternalLogger logger, CancellationToken token)
    {
        if (SnmpTestOverride != null)
        {
            return await SnmpTestOverride(host, port);
        }

        try
        {
            using var udp = new UdpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(Timeout);
#if NET8_0_OR_GREATER
            await udp.SendAsync(_probe, host, port, cts.Token);
            var result = await udp.ReceiveAsync(cts.Token);
#else
            await udp.SendAsync(_probe, _probe.Length, host, port).WaitWithCancellation(cts.Token);
            var result = await udp.ReceiveAsync().WaitWithCancellation(cts.Token);
#endif
            return result.Buffer.Length > 0;
        }
        catch (TaskCanceledException ex)
        {
            throw new OperationCanceledException(ex.Message, ex, token);
        }
        catch (Exception ex)
        {
            logger?.WriteVerbose("SNMP query failed for {0}:{1} - {2}", host, port, ex.Message);
            return false;
        }
    }
}
