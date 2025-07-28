namespace DomainDetective.Reports.Enums;

/// <summary>
/// Implementation effort levels for recommendations
/// </summary>
public enum EffortLevel {
    /// <summary>
    /// Minimal effort - Quick configuration change
    /// </summary>
    Minimal,
    
    /// <summary>
    /// Low effort - Simple implementation
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium effort - Moderate implementation work
    /// </summary>
    Medium,
    
    /// <summary>
    /// High effort - Significant implementation work
    /// </summary>
    High,
    
    /// <summary>
    /// Complex effort - Major project or architectural change
    /// </summary>
    Complex
}