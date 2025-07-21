using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Scans common directories for public exposure.
        /// </summary>
        public async Task VerifyDirectoryExposure(string domainName, CancellationToken cancellationToken = default) {
            domainName = ValidateHostName(domainName);
            UpdateIsPublicSuffix(domainName);
            DirectoryExposureAnalysis = new DirectoryExposureAnalysis();
            await DirectoryExposureAnalysis.Analyze($"http://{domainName}", _logger, cancellationToken);
        }
    }
}
