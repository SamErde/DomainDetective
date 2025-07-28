namespace DomainDetective.Reports.Enums;

/// <summary>
/// Status of individual security checks
/// </summary>
public enum CheckStatus {
    /// <summary>
    /// Check passed successfully
    /// </summary>
    Pass,
    
    /// <summary>
    /// Check passed with warnings
    /// </summary>
    Warning,
    
    /// <summary>
    /// Check failed
    /// </summary>
    Fail,
    
    /// <summary>
    /// Check is not applicable for this domain
    /// </summary>
    NotApplicable,
    
    /// <summary>
    /// Error occurred during check
    /// </summary>
    Error,
    
    /// <summary>
    /// Check was skipped
    /// </summary>
    Skipped
}