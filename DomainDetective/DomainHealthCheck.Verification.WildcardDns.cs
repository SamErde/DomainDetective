using System;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Queries random subdomains to detect wildcard DNS behavior.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="sampleCount">Number of names to test.</param>
        public async Task VerifyWildcardDns(string domainName, int sampleCount = 3) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            WildcardDnsAnalysis.DnsConfiguration = DnsConfiguration;
            await WildcardDnsAnalysis.Analyze(domainName, _logger, sampleCount);
        }
    }
}
