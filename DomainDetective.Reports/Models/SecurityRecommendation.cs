using System.Collections.Generic;
using DomainDetective.Reports.Enums;

namespace DomainDetective.Reports.Models;

/// <summary>
/// Represents a security recommendation
/// </summary>
public class SecurityRecommendation {
    /// <summary>
    /// Recommendation priority
    /// </summary>
    public RecommendationPriority Priority { get; set; }
    
    /// <summary>
    /// Recommendation title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Implementation steps
    /// </summary>
    public List<string> Steps { get; set; } = new();
    
    /// <summary>
    /// Expected impact when implemented
    /// </summary>
    public string ExpectedImpact { get; set; } = string.Empty;
    
    /// <summary>
    /// Estimated effort level
    /// </summary>
    public EffortLevel Effort { get; set; }
    
    /// <summary>
    /// Points that would be gained
    /// </summary>
    public int PotentialScoreIncrease { get; set; }
}