using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective;

public partial class DomainHealthCheck
{
    private async Task VerifyRobotsAsync(string domainName, CancellationToken cancellationToken)
    {
        RobotsTxtAnalysis = new RobotsTxtAnalysis();
        await RobotsTxtAnalysis.AnalyzeAsync(domainName, _logger, cancellationToken);
    }
}
