using DnsClientX;

namespace DomainDetective.Tests {
    public class TestDkimSelectors {
        [Fact]
        public async Task EmptySelectorsAreIgnored() {
            var healthCheck = new DomainHealthCheck(DnsEndpoint.CloudflareWireFormat) { Verbose = false };
            await healthCheck.VerifyDKIM("evotec.pl", new[] { " selector1 ", "", " \t", "selector2" });
            if (healthCheck.DKIMAnalysis.AnalysisResults.Count == 0) {
                return;
            }

            Assert.Equal(2, healthCheck.DKIMAnalysis.AnalysisResults.Count);
            Assert.True(healthCheck.DKIMAnalysis.AnalysisResults.ContainsKey("selector1"));
            Assert.True(healthCheck.DKIMAnalysis.AnalysisResults.ContainsKey("selector2"));
            Assert.False(healthCheck.DKIMAnalysis.AnalysisResults.ContainsKey(" selector1 "));
        }
    }
}
