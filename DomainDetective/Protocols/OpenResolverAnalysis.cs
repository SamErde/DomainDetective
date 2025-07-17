using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective;

/// <summary>
/// Checks if DNS servers allow recursive queries.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public class OpenResolverAnalysis {
    /// <summary>Recursion results keyed by server and port.</summary>
    public Dictionary<string, bool> ServerResults { get; private set; } = new();

    /// <summary>Maximum wait time for each query.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    internal Func<string, int, Task<bool>>? RecursionTestOverride { get; set; }

    /// <summary>Tests a single server for open recursion.</summary>
    public async Task AnalyzeServer(string host, int port, InternalLogger logger, CancellationToken cancellationToken = default) {
        ServerResults.Clear();
        var result = await CheckRecursionAsync(host, port, logger, cancellationToken);
        ServerResults[$"{host}:{port}"] = result;
    }

    /// <summary>Tests multiple servers and ports for open recursion.</summary>
    public async Task AnalyzeServers(IEnumerable<string> hosts, IEnumerable<int> ports, InternalLogger logger, CancellationToken cancellationToken = default) {
        ServerResults.Clear();
        foreach (var host in hosts) {
            foreach (var port in ports) {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await CheckRecursionAsync(host, port, logger, cancellationToken);
                ServerResults[$"{host}:{port}"] = result;
            }
        }
    }

    private static byte[] EncodeDomainName(string name) {
        var parts = name.TrimEnd('.').Split('.');
        using var ms = new System.IO.MemoryStream();
        foreach (var part in parts) {
            var bytes = System.Text.Encoding.ASCII.GetBytes(part);
            ms.WriteByte((byte)bytes.Length);
            ms.Write(bytes, 0, bytes.Length);
        }
        ms.WriteByte(0);
        return ms.ToArray();
    }

    private static byte[] BuildQuery(string domain, ushort id) {
        var header = new byte[12];
        header[0] = (byte)(id >> 8);
        header[1] = (byte)id;
        header[2] = 0x01; // recursion desired
        header[5] = 0x01; // qdcount
        var qname = EncodeDomainName(domain);
        var query = new byte[header.Length + qname.Length + 4];
        Buffer.BlockCopy(header, 0, query, 0, header.Length);
        Buffer.BlockCopy(qname, 0, query, header.Length, qname.Length);
        var offset = header.Length + qname.Length;
        query[offset] = 0x00;
        query[offset + 1] = 0x01; // A
        query[offset + 2] = 0x00;
        query[offset + 3] = 0x01; // IN
        return query;
    }

    private async Task<bool> CheckRecursionAsync(string server, int port, InternalLogger logger, CancellationToken token) {
        if (RecursionTestOverride != null) {
            return await RecursionTestOverride(server, port);
        }

        try {
            using var udp = new UdpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(Timeout);
            var id = (ushort)new Random().Next(ushort.MaxValue);
            var query = BuildQuery("example.com", id);
#if NET8_0_OR_GREATER
            await udp.SendAsync(query, server, port, cts.Token);
            var result = await udp.ReceiveAsync(cts.Token);
#else
            await udp.SendAsync(query, query.Length, server, port).WaitWithCancellation(cts.Token);
            var result = await udp.ReceiveAsync().WaitWithCancellation(cts.Token);
#endif
            var data = result.Buffer;
            return data.Length > 3 && (data[3] & 0x80) != 0 && (data[3] & 0x0F) == 0;
        } catch (TaskCanceledException ex) {
            throw new OperationCanceledException(ex.Message, ex, token);
        } catch (Exception ex) {
            logger?.WriteVerbose("Recursion test failed for {0}:{1} - {2}", server, port, ex.Message);
            return false;
        }
    }
}