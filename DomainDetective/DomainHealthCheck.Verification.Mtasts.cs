using System;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Verifies MTA-STS policy for a domain.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyMTASTS(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            MTASTSAnalysis = new MTASTSAnalysis {
                PolicyUrlOverride = MtaStsPolicyUrlOverride,
                DnsConfiguration = DnsConfiguration
            };
            await MTASTSAnalysis.AnalyzePolicy(domainName, _logger);
        }
    }
}
