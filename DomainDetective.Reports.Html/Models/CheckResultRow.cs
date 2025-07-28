namespace DomainDetective.Reports.Html.Models;

/// <summary>
/// Represents a row in the security check results table
/// </summary>
internal class CheckResultRow {
    public string Category { get; set; } = "";
    public string Check { get; set; } = "";
    public string Status { get; set; } = "";
    public string Points { get; set; } = "";
    public string Details { get; set; } = "";
}