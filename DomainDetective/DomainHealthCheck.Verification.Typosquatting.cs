using DnsClientX;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Generates typosquatting variants and checks if they resolve.
        /// </summary>
        /// <param name="domainName">Domain to analyze.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyTyposquatting(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            TyposquattingAnalysis.DnsConfiguration = DnsConfiguration;
            TyposquattingAnalysis.LevenshteinThreshold = TyposquattingLevenshteinThreshold;
            TyposquattingAnalysis.DetectHomoglyphs = EnableHomoglyphDetection;
            TyposquattingAnalysis.BrandKeywords.Clear();
            TyposquattingAnalysis.BrandKeywords.AddRange(TyposquattingBrandKeywords);
            await TyposquattingAnalysis.Analyze(domainName, _logger, cancellationToken);
        }
    }
}
