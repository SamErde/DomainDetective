using System;
using System.Threading.Tasks;

namespace DomainDetective.Tests {
    public class TestUnknownHealthCheckType {
        [Fact]
        public async Task VerifyUnknownHealthCheckTypeThrows() {
            var healthCheck = new DomainHealthCheck();
            var ex = await Assert.ThrowsAsync<NotSupportedException>(async () =>
                await healthCheck.Verify("example.com", new[] { (HealthCheckType)999 }));
            Assert.Equal("Health check type not implemented: 999", ex.Message);
        }
    }
}