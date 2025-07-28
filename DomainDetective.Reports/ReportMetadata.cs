using System;
using System.Collections.Generic;

namespace DomainDetective.Reports;

/// <summary>
/// Metadata about a generated report
/// </summary>
public class ReportMetadata {
    /// <summary>
    /// Unique identifier for the report
    /// </summary>
    public string ReportId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Domain analyzed in the report
    /// </summary>
    public string Domain { get; set; } = string.Empty;
    
    /// <summary>
    /// Template used for generation
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;
    
    /// <summary>
    /// Version of the report generator
    /// </summary>
    public string GeneratorVersion { get; set; } = "1.0.0";
    
    /// <summary>
    /// Number of checks performed
    /// </summary>
    public int TotalChecks { get; set; }
    
    /// <summary>
    /// Number of passed checks
    /// </summary>
    public int PassedChecks { get; set; }
    
    /// <summary>
    /// Number of failed checks
    /// </summary>
    public int FailedChecks { get; set; }
    
    /// <summary>
    /// Overall security score
    /// </summary>
    public int SecurityScore { get; set; }
    
    /// <summary>
    /// Custom properties specific to the report type
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}