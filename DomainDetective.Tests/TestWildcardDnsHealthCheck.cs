using DnsClientX;
using System.Threading.Tasks;

namespace DomainDetective.Tests;

public class TestWildcardDnsHealthCheck
{
    [Fact]
    public async Task DetectsWildcardThroughHealthCheck()
    {
        var hc = new DomainHealthCheck();
        hc.WildcardDnsAnalysis.QueryDnsOverride = (_, _) => Task.FromResult(new[] { new DnsAnswer { Type = DnsRecordType.A, DataRaw = "192.0.2.1" } });
        // sslip.io returns the client's IP for any subdomain, making it a convenient
        // domain to demonstrate wildcard DNS behavior.
        await hc.VerifyWildcardDns("sslip.io");
        Assert.True(hc.WildcardDnsAnalysis.CatchAll);
    }
}
