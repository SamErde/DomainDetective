using System;
using System.Management.Automation;
using System.Threading.Tasks;
using DomainDetective.Reports;
using DomainDetective.Reports.Html;

namespace DomainDetective.PowerShell;

/// <summary>
/// Displays a domain security report in various formats
/// </summary>
/// <example>
/// <code>
/// Show-DDDomainReport -Domain "example.com" -Format Html -Path "report.html"
/// Show-DDDomainReport -Domain "example.com" -Format Html -Template Executive -OpenInBrowser
/// Show-DDDomainReport -Domain "example.com" -Format Word -IncludeRawData
/// </code>
/// </example>
[Cmdlet(VerbsCommon.Show, "DDDomainReport")]
[OutputType(typeof(ReportResult))]
[Alias("Export-DomainReport", "New-DomainReport", "Generate-DomainReport")]
public sealed class CmdletShowDDDomainReport : AsyncPSCmdlet {
    /// <summary>
    /// Domain name to analyze and report on
    /// </summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    public string Domain { get; set; } = string.Empty;
    
    /// <summary>
    /// Output format for the report
    /// </summary>
    [Parameter(Position = 1)]
    [ValidateSet("Html", "Word", "Excel", "PDF", "Json", "Csv")]
    public string Format { get; set; } = "Html";
    
    /// <summary>
    /// Output file path
    /// </summary>
    [Parameter(Position = 2)]
    [Alias("OutputPath", "FilePath")]
    public string? Path { get; set; }
    
    /// <summary>
    /// Report template to use
    /// </summary>
    [Parameter]
    [ValidateSet("Default", "Executive", "Technical", "Compliance")]
    public string Template { get; set; } = "Default";
    
    /// <summary>
    /// Report theme
    /// </summary>
    [Parameter]
    [ValidateSet("Light", "Dark", "Professional", "HighContrast")]
    public string Theme { get; set; } = "Light";
    
    /// <summary>
    /// Include technical details in the report
    /// </summary>
    [Parameter]
    public SwitchParameter IncludeTechnicalDetails { get; set; } = true;
    
    /// <summary>
    /// Include recommendations in the report
    /// </summary>
    [Parameter]
    public SwitchParameter IncludeRecommendations { get; set; } = true;
    
    /// <summary>
    /// Include raw data export
    /// </summary>
    [Parameter]
    public SwitchParameter IncludeRawData { get; set; }
    
    /// <summary>
    /// Open the report in browser (HTML only)
    /// </summary>
    [Parameter]
    public SwitchParameter OpenInBrowser { get; set; }
    
    /// <summary>
    /// Use existing DomainHealthCheck object
    /// </summary>
    [Parameter(ValueFromPipeline = true)]
    public DomainHealthCheck? HealthCheck { get; set; }
    
    protected override async Task ProcessRecordAsync() {
        try {
            // Step 1: Get or create health check
            if (HealthCheck == null) {
                WriteVerbose($"Performing health check for {Domain}");
                HealthCheck = new DomainHealthCheck();
                
                await HealthCheck.Verify(Domain);
            } else {
                // Domain must be specified when using existing health check
                if (string.IsNullOrEmpty(Domain)) {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("Domain parameter is required when using existing health check"),
                        "MissingDomain",
                        ErrorCategory.InvalidArgument,
                        null));
                }
            }
            
            // Step 2: Determine output path
            Path ??= GenerateDefaultPath();
            
            // Step 3: Create report options
            var options = new ReportOptions {
                Title = $"Security Report - {Domain}",
                OutputPath = Path,
                TemplateName = Template,
                Theme = (ReportTheme)Enum.Parse(typeof(ReportTheme), Theme),
                IncludeTechnicalDetails = IncludeTechnicalDetails.IsPresent,
                IncludeRecommendations = IncludeRecommendations.IsPresent,
                IncludeRawData = IncludeRawData.IsPresent
            };
            
            // Step 4: Generate report based on format
            ReportResult result;
            switch (Format.ToUpper()) {
                case "HTML":
                    WriteVerbose("Generating HTML report...");
                    var htmlReport = new DomainSecurityReport(HealthCheck, Domain);
                    htmlReport.GenerateReport(Path, OpenInBrowser.IsPresent);
                    
                    result = new ReportResult {
                        Success = true,
                        FilePath = Path,
                        Format = ReportFormat.Html,
                        GenerationTime = TimeSpan.FromSeconds(1) // Placeholder
                    };
                    break;
                    
                case "WORD":
                    WriteWarning("Word format not yet implemented");
                    result = new ReportResult {
                        Success = false,
                        ErrorMessage = "Word format not yet implemented",
                        Format = ReportFormat.Word
                    };
                    break;
                    
                case "EXCEL":
                    WriteWarning("Excel format not yet implemented");
                    result = new ReportResult {
                        Success = false,
                        ErrorMessage = "Excel format not yet implemented",
                        Format = ReportFormat.Excel
                    };
                    break;
                    
                case "PDF":
                    WriteWarning("PDF format not yet implemented");
                    result = new ReportResult {
                        Success = false,
                        ErrorMessage = "PDF format not yet implemented",
                        Format = ReportFormat.Pdf
                    };
                    break;
                    
                case "JSON":
                    WriteVerbose("Exporting to JSON...");
                    var json = HealthCheck.ToJson();
#if NET472
                    System.IO.File.WriteAllText(Path, json);
#else
                    await System.IO.File.WriteAllTextAsync(Path, json);
#endif
                    result = new ReportResult {
                        Success = true,
                        FilePath = Path,
                        Format = ReportFormat.Json,
                        FileSize = json.Length
                    };
                    break;
                    
                case "CSV":
                    WriteWarning("CSV format not yet implemented");
                    result = new ReportResult {
                        Success = false,
                        ErrorMessage = "CSV format not yet implemented",
                        Format = ReportFormat.Csv
                    };
                    break;
                    
                default:
                    throw new ArgumentException($"Unknown format: {Format}");
            }
            
            // Step 5: Output result
            if (result.Success) {
                WriteObject(result);
                WriteInformation($"Report generated successfully: {result.FilePath}", new string[] { "ReportGenerated" });
            } else {
                WriteError(new ErrorRecord(
                    new InvalidOperationException(result.ErrorMessage),
                    "ReportGenerationFailed",
                    ErrorCategory.NotImplemented,
                    Format
                ));
            }
            
        } catch (Exception ex) {
            WriteError(new ErrorRecord(
                ex,
                "ReportGenerationError",
                ErrorCategory.WriteError,
                Domain
            ));
        }
    }
    
    private string GenerateDefaultPath() {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var extension = Format.ToLower() switch {
            "html" => "html",
            "word" => "docx",
            "excel" => "xlsx",
            "pdf" => "pdf",
            "json" => "json",
            "csv" => "csv",
            _ => "html"
        };
        
        return $"{Domain.Replace(".", "_")}_{timestamp}.{extension}";
    }
}