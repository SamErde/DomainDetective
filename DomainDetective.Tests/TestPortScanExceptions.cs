using System;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective.Tests;

public class TestPortScanExceptions
{
    [Fact]
    public async Task CancellationPropagates()
    {
        var analysis = new PortScanAnalysis();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await analysis.Scan("127.0.0.1", new[] { 80 }, new InternalLogger(), cts.Token));
    }
}
