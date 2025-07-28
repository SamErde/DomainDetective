using System.Threading.Tasks;

namespace DomainDetective.Reports;

/// <summary>
/// Common interface for all report generators
/// </summary>
public interface IReportGenerator {
    /// <summary>
    /// Generates a report for the given health check
    /// </summary>
    Task<ReportResult> GenerateAsync(DomainHealthCheck healthCheck, ReportOptions options);
    
    /// <summary>
    /// Gets the supported output format
    /// </summary>
    ReportFormat Format { get; }
    
    /// <summary>
    /// Checks if the generator can handle the requested options
    /// </summary>
    bool CanGenerate(ReportOptions options);
}