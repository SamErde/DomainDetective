using System;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Verifies the certificate for a website. If no scheme is provided in <paramref name="url"/>, "https://" is assumed.
        /// </summary>
        public async Task VerifyWebsiteCertificate(string url, int port = 443, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                url = $"https://{url}";
            }
            await CertificateAnalysis.AnalyzeUrl(url, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Performs a basic HTTP check without enforcing HTTPS.
        /// </summary>
        public async Task VerifyPlainHttp(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = ValidateHostName(domainName);
            UpdateIsPublicSuffix(domainName);
            await HttpAnalysis.AnalyzeUrl($"http://{domainName}", false, _logger, cancellationToken: cancellationToken);
        }
    }
}
