namespace DomainDetective.Reports;

/// <summary>
/// Supported report output formats
/// </summary>
public enum ReportFormat {
    /// <summary>
    /// HTML report with interactive elements
    /// </summary>
    Html,
    
    /// <summary>
    /// Microsoft Word document
    /// </summary>
    Word,
    
    /// <summary>
    /// Microsoft Excel spreadsheet
    /// </summary>
    Excel,
    
    /// <summary>
    /// Portable Document Format
    /// </summary>
    Pdf,
    
    /// <summary>
    /// JSON data export
    /// </summary>
    Json,
    
    /// <summary>
    /// CSV data export
    /// </summary>
    Csv,
    
    /// <summary>
    /// Markdown documentation
    /// </summary>
    Markdown
}