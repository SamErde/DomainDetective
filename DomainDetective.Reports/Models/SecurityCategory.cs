using System.Collections.Generic;
using DomainDetective.Reports.Enums;

namespace DomainDetective.Reports.Models;

/// <summary>
/// Represents a security category with its score and details
/// </summary>
public class SecurityCategory {
    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Category score (0-5)
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// Maximum possible score for this category
    /// </summary>
    public int MaxScore { get; set; } = 5;
    
    /// <summary>
    /// Risk level for this category
    /// </summary>
    public SecurityRiskLevel Risk { get; set; }
    
    /// <summary>
    /// Description of what this category measures
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Individual checks within this category
    /// </summary>
    public List<SecurityCheck> Checks { get; set; } = new();
}