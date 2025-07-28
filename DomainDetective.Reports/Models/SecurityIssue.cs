using DomainDetective.Reports.Enums;

namespace DomainDetective.Reports.Models;

/// <summary>
/// Represents a security issue found during analysis
/// </summary>
public class SecurityIssue {
    /// <summary>
    /// Issue severity
    /// </summary>
    public IssueSeverity Severity { get; set; }
    
    /// <summary>
    /// Issue category
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Issue title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Impact if not resolved
    /// </summary>
    public string Impact { get; set; } = string.Empty;
    
    /// <summary>
    /// Associated health check type
    /// </summary>
    public HealthCheckType? HealthCheckType { get; set; }
}