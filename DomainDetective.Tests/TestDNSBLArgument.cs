using System;
using System.Threading.Tasks;

namespace DomainDetective.Tests {
    public class TestDnsblArgument {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CheckDnsblThrowsIfAddressNullOrWhitespace(string? address) {
            var healthCheck = new DomainHealthCheck();
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await healthCheck.CheckDNSBL(address!));
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("256.256.256.256")]
        public async Task CheckDnsblThrowsIfAddressInvalid(string address) {
            var healthCheck = new DomainHealthCheck();
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await healthCheck.CheckDNSBL(address));
        }

        [Fact]
        public async Task CheckDnsblArrayThrowsIfAddressInvalid() {
            var healthCheck = new DomainHealthCheck();
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await healthCheck.CheckDNSBL(new[] { "127.0.0.1", "invalid" }));
        }
    }
}
