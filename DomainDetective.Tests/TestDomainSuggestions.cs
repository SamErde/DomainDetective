using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DomainDetective.Tests;

public class TestDomainSuggestions
{
    [Fact]
    public async Task SuggestsAvailableAlternative()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var serverTask = Task.Run(async () =>
        {
            for (int i = 0; i < 4; i++)
            {
                using var client = await listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);
                var line = await reader.ReadLineAsync();
                using var writer = new StreamWriter(stream) { AutoFlush = true, NewLine = "\r\n" };
                if (line != null && line.Contains("test.net"))
                {
                    await writer.WriteAsync("NOT FOUND");
                }
                else
                {
                    await writer.WriteAsync("Domain Name: test.com");
                }
            }
        });

        try
        {
            var search = new DomainAvailabilitySearch
            {
                AvailabilityOverride = async (d, _) =>
                {
                    using var tcp = new TcpClient();
                    await tcp.ConnectAsync(IPAddress.Loopback, port);
                    using var s = tcp.GetStream();
                    using var w = new StreamWriter(s) { AutoFlush = true, NewLine = "\r\n" };
                    await w.WriteLineAsync(d);
                    using var r = new StreamReader(s);
                    var resp = await r.ReadToEndAsync();
                    return resp.Contains("NOT FOUND");
                }
            };

            var results = new List<DomainAvailabilityResult>();
            await foreach (var r in search.CheckTldAlternativesAsync("test.com"))
            {
                results.Add(r);
            }

            Assert.Contains(results, r => r.Domain == "test.net" && r.Available);
        }
        finally
        {
            listener.Stop();
            await serverTask;
        }
    }
}
