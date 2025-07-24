using DnsClientX;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DomainDetective.Tests {
    public class TestVerifySmtpTls {
        private static DomainHealthCheck CreateHealthCheck() {
            var hc = new DomainHealthCheck();
            hc.DnsConfiguration = new DnsConfiguration {
                QueryDnsOverride = (name, type) => {
                    if (type == DnsRecordType.MX) {
                        return Task.FromResult(new[] { new DnsAnswer { DataRaw = "0 localhost" } });
                    }
                    return Task.FromResult(Array.Empty<DnsAnswer>());
                }
            };
            return hc;
        }

        [Fact]
        public async Task DefaultPortIs25() {
            var hc = CreateHealthCheck();
            await hc.VerifySMTPTLS("example.com");
            Assert.Contains("localhost:25", hc.SmtpTlsAnalysis.ServerResults.Keys);
        }

        [Fact]
        public async Task CustomPortRespected() {
            var port = PortHelper.GetFreePort();
            var hc = CreateHealthCheck();
            await hc.VerifySMTPTLS("example.com", port);
            Assert.Contains($"localhost:{port}", hc.SmtpTlsAnalysis.ServerResults.Keys);
            PortHelper.ReleasePort(port);
        }
    }
}