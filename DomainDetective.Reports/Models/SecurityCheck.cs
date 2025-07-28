using DomainDetective.Reports.Enums;

namespace DomainDetective.Reports.Models;

/// <summary>
/// Represents an individual security check
/// </summary>
public class SecurityCheck {
    /// <summary>
    /// Check name (e.g., "SPF Record")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Check status
    /// </summary>
    public CheckStatus Status { get; set; }
    
    /// <summary>
    /// Points awarded for this check
    /// </summary>
    public int Points { get; set; }
    
    /// <summary>
    /// Maximum points possible for this check
    /// </summary>
    public int MaxPoints { get; set; }
    
    /// <summary>
    /// Detailed message about the check result
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Associated health check type
    /// </summary>
    public HealthCheckType? HealthCheckType { get; set; }
}