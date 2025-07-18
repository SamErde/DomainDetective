using System;
using DomainDetective;
using DnsClientX;

namespace DomainDetective.Tests {
    public class TestDomainHealthCheckConstructor {
        [Fact]
        public void DefaultEndpointUsesSystemDns() {
            var healthCheck = new DomainHealthCheck(default);
            Assert.Equal(DnsEndpoint.System, healthCheck.DnsEndpoint);
        }
    }
}
