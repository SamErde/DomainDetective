namespace DomainDetective.Reports.Enums;

/// <summary>
/// Severity levels for security issues
/// </summary>
public enum IssueSeverity {
    /// <summary>
    /// Low severity - Minor issue
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium severity - Should be addressed
    /// </summary>
    Medium,
    
    /// <summary>
    /// High severity - Important to fix
    /// </summary>
    High,
    
    /// <summary>
    /// Critical severity - Must be fixed immediately
    /// </summary>
    Critical
}