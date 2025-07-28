namespace DomainDetective.Reports.Enums;

/// <summary>
/// Priority levels for security recommendations
/// </summary>
public enum RecommendationPriority {
    /// <summary>
    /// Low priority - Can be addressed later
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium priority - Should be planned
    /// </summary>
    Medium,
    
    /// <summary>
    /// High priority - Should be addressed soon
    /// </summary>
    High,
    
    /// <summary>
    /// Urgent priority - Requires immediate attention
    /// </summary>
    Urgent
}