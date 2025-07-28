using System;
using System.IO;
using System.Threading.Tasks;
using DomainDetective;
using DomainDetective.Reports;
using DomainDetective.Reports.Html;

namespace DomainDetective.Example;

/// <summary>
/// Example demonstrating how to generate HTML reports for domain security analysis
/// </summary>
internal class ReportingHtmlExample {
    public static async Task Run() {
        Console.WriteLine("\n=== HTML Report Generation Demo ===");
        Console.WriteLine("================================\n");

        // Create output directory for reports
        var reportsDir = "Reports";
        Directory.CreateDirectory(reportsDir);

        // Test domain
        var domain = "github.com";

        Console.WriteLine($"Analyzing domain: {domain}");

        // Run analysis
        var healthCheck = new DomainHealthCheck();
        await healthCheck.Verify(domain);

        Console.WriteLine("\nGenerating reports...");

        // Generate Basic Report
        Console.WriteLine("1. Generating Basic Domain Report...");
        var basicReport = new BasicDomainReport(healthCheck, domain);
        var basicPath = Path.Combine(reportsDir, "BasicDomainReport.html");
        basicReport.GenerateReport(basicPath, openInBrowser: false);
        Console.WriteLine($"   ✓ {basicPath} created");

        // Generate Simple Report
        Console.WriteLine("2. Generating Simple Domain Report...");
        var simpleReport = new SimpleDomainReport(healthCheck, domain);
        var simplePath = Path.Combine(reportsDir, "SimpleDomainReport.html");
        simpleReport.GenerateReport(simplePath, openInBrowser: false);
        Console.WriteLine($"   ✓ {simplePath} created");

        // Generate Advanced Report
        Console.WriteLine("3. Generating Advanced Security Report...");
        var advancedReport = new DomainSecurityReport(healthCheck, domain);
        var advancedPath = Path.Combine(reportsDir, "DomainSecurityReport.html");
        advancedReport.GenerateReport(advancedPath, openInBrowser: true);
        Console.WriteLine($"   ✓ {advancedPath} created and opened in browser");

        Console.WriteLine("\nDemo completed successfully!");
        Console.WriteLine($"Reports have been generated in the '{reportsDir}' directory.");

        // Show additional demos
        await DemoWithOptions();
        await DemoBatchReporting();
        DemoScoringSystem();
    }

    private static async Task DemoWithOptions() {
        Console.WriteLine("\n=== Advanced Report Generation ===");

        // This demonstrates the planned API
        var healthCheck = new DomainHealthCheck();
        await healthCheck.Verify("example.com");

        // Create report options
        var options = new ReportOptions {
            Title = "Executive Security Report",
            OutputPath = "executive_report.html",
            TemplateName = "Executive",
            Theme = ReportTheme.Professional,
            IncludeTechnicalDetails = false,
            IncludeRecommendations = true,
            CustomProperties = new() {
                ["ShowTrends"] = true,
                ["ComparisonDomains"] = new[] { "competitor1.com", "competitor2.com" }
            }
        };

        // Future: Use report generator factory
        // var generator = ReportGeneratorFactory.Create(ReportFormat.Html);
        // var result = await generator.GenerateAsync(healthCheck, options);

        Console.WriteLine("Advanced report features:");
        Console.WriteLine("- Custom templates (Executive, Technical, Compliance)");
        Console.WriteLine("- Theme selection (Light, Dark, Professional)");
        Console.WriteLine("- Configurable sections");
        Console.WriteLine("- Export to multiple formats");
    }

    public static async Task DemoBatchReporting() {
        Console.WriteLine("\n=== Batch Report Generation ===");

        var domains = new[] { "example.com", "google.com", "microsoft.com" };
        var reportsDir = "Reports/Batch";
        Directory.CreateDirectory(reportsDir);

        foreach (var domain in domains) {
            try {
                Console.WriteLine($"\nProcessing {domain}...");
                var healthCheck = new DomainHealthCheck();
                await healthCheck.Verify(domain);

                // Generate all three report types for each domain
                var basicReport = new BasicDomainReport(healthCheck, domain);
                basicReport.GenerateReport(Path.Combine(reportsDir, $"{domain}_basic.html"), openInBrowser: false);

                var simpleReport = new SimpleDomainReport(healthCheck, domain);
                simpleReport.GenerateReport(Path.Combine(reportsDir, $"{domain}_simple.html"), openInBrowser: false);

                var advancedReport = new DomainSecurityReport(healthCheck, domain);
                advancedReport.GenerateReport(Path.Combine(reportsDir, $"{domain}_advanced.html"), openInBrowser: false);

                Console.WriteLine($"✓ Generated all reports for {domain}");
            }
            catch (Exception ex) {
                Console.WriteLine($"✗ Failed to generate reports for {domain}: {ex.Message}");
            }
        }

        Console.WriteLine($"\nBatch reports saved to '{reportsDir}' directory.");
    }

    public static void DemoScoringSystem() {
        Console.WriteLine("\n=== Scoring System Demo ===");

        // Show how the scoring works
        Console.WriteLine("Security Score Calculation:");
        Console.WriteLine("- SPF: 10 points");
        Console.WriteLine("- DMARC: 15 points");
        Console.WriteLine("- DKIM: 10 points");
        Console.WriteLine("- DNSSEC: 10 points");
        Console.WriteLine("- TLS/SSL: 15 points");
        Console.WriteLine("- MTA-STS: 10 points");
        Console.WriteLine("- DANE: 5 points");
        Console.WriteLine("- BIMI: 5 points");
        Console.WriteLine("- CAA: 5 points");
        Console.WriteLine("- Security Headers: 10 points");
        Console.WriteLine("- Other: 5 points");
        Console.WriteLine("\nTotal: 100 points");

        Console.WriteLine("\nRisk Levels:");
        Console.WriteLine("- 80-100: Low Risk (Green)");
        Console.WriteLine("- 60-79: Medium Risk (Yellow)");
        Console.WriteLine("- 40-59: High Risk (Orange)");
        Console.WriteLine("- 0-39: Critical Risk (Red)");
    }
}