using System;

namespace DomainDetective.Tests {
    public class TestMtaStsPolicyUrlOverride {
        [Fact]
        public void AcceptsAbsoluteUri() {
            var hc = new DomainHealthCheck();
            const string url = "https://example.com/policy.txt";
            hc.MtaStsPolicyUrlOverride = url;
            Assert.Equal(url, hc.MtaStsPolicyUrlOverride);
        }

        [Fact]
        public void ThrowsOnRelativeUri() {
            var hc = new DomainHealthCheck();
            Assert.Throws<ArgumentException>(() => hc.MtaStsPolicyUrlOverride = "policy.txt");
        }
    }
}
