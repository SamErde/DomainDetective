using System;
using System.Collections.Generic;
using DomainDetective.Reports.Enums;

namespace DomainDetective.Reports.Models;

/// <summary>
/// Represents the overall security score for a domain with category breakdowns
/// </summary>
public class SecurityScore {
    /// <summary>
    /// Overall domain security score (0-100)
    /// </summary>
    public int OverallScore { get; set; }
    
    /// <summary>
    /// Risk level based on overall score
    /// </summary>
    public SecurityRiskLevel RiskLevel { get; set; }
    
    /// <summary>
    /// Impersonation protection score (SPF, DMARC, DKIM)
    /// </summary>
    public SecurityCategory Impersonation { get; set; } = new();
    
    /// <summary>
    /// Privacy and encryption score (TLS, DANE, MTA-STS)
    /// </summary>
    public SecurityCategory Privacy { get; set; } = new();
    
    /// <summary>
    /// Brand protection score (BIMI, Certificate)
    /// </summary>
    public SecurityCategory Branding { get; set; } = new();
    
    /// <summary>
    /// Infrastructure security score (DNSSEC, NS, MX)
    /// </summary>
    public SecurityCategory Infrastructure { get; set; } = new();
    
    /// <summary>
    /// List of identified security issues
    /// </summary>
    public List<SecurityIssue> Issues { get; set; } = new();
    
    /// <summary>
    /// Prioritized security recommendations
    /// </summary>
    public List<SecurityRecommendation> Recommendations { get; set; } = new();
    
    /// <summary>
    /// Timestamp when the score was calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}