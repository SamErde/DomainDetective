namespace DomainDetective.Reports.Enums;

/// <summary>
/// Security risk levels for domain assessment
/// </summary>
public enum SecurityRiskLevel {
    /// <summary>
    /// Low risk - Good security posture
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium risk - Some improvements needed
    /// </summary>
    Medium,
    
    /// <summary>
    /// High risk - Significant vulnerabilities
    /// </summary>
    High,
    
    /// <summary>
    /// Critical risk - Immediate action required
    /// </summary>
    Critical
}