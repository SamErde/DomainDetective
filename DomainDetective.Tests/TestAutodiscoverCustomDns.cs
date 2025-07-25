using DnsClientX;
using System.Threading.Tasks;

namespace DomainDetective.Tests {
    public class TestAutodiscoverCustomDns {
        [Fact]
        public async Task AutodiscoverUsesCustomDnsEndpoint() {
            var hc = new DomainHealthCheck(DnsEndpoint.CloudflareWireFormat);
            hc.DnsConfiguration.QueryDnsOverride = (_, _) => Task.FromResult(System.Array.Empty<DnsAnswer>());
            await hc.VerifyAutodiscover("example.com");
            Assert.Same(hc.DnsConfiguration, hc.AutodiscoverAnalysis.DnsConfiguration);
            Assert.Equal(DnsEndpoint.CloudflareWireFormat, hc.AutodiscoverAnalysis.DnsConfiguration.DnsEndpoint);
        }
    }
}
