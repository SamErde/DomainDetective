using System;

namespace DomainDetective.Reports;

/// <summary>
/// Result of report generation
/// </summary>
public class ReportResult {
    /// <summary>
    /// Indicates if the report was generated successfully
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Path to the generated report file
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Format of the generated report
    /// </summary>
    public ReportFormat Format { get; set; }
    
    /// <summary>
    /// Size of the generated file in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Time taken to generate the report
    /// </summary>
    public TimeSpan GenerationTime { get; set; }
    
    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Additional metadata about the report
    /// </summary>
    public ReportMetadata? Metadata { get; set; }
}