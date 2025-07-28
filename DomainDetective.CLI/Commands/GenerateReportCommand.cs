using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using DomainDetective.Reports;
using DomainDetective.Reports.Html;

namespace DomainDetective.CLI.Commands;

/// <summary>
/// CLI command for generating domain security reports
/// </summary>
internal sealed class GenerateReportCommand : AsyncCommand<GenerateReportCommand.Settings> {
    public sealed class Settings : CommandSettings {
        [Description("Domain to analyze")]
        [CommandArgument(0, "<domain>")]
        public string Domain { get; set; } = string.Empty;
        
        [Description("Output format (html, word, excel, pdf)")]
        [CommandOption("-f|--format")]
        [DefaultValue("html")]
        public string Format { get; set; } = "html";
        
        [Description("Output file path")]
        [CommandOption("-o|--output")]
        public string? OutputPath { get; set; }
        
        [Description("Report template (default, executive, technical, compliance)")]
        [CommandOption("-t|--template")]
        [DefaultValue("default")]
        public string Template { get; set; } = "default";
        
        [Description("Report theme (light, dark, professional)")]
        [CommandOption("--theme")]
        [DefaultValue("light")]
        public string Theme { get; set; } = "light";
        
        [Description("Open report in browser after generation")]
        [CommandOption("--open")]
        [DefaultValue(true)]
        public bool OpenInBrowser { get; set; } = true;
        
        [Description("Include technical details")]
        [CommandOption("--technical")]
        [DefaultValue(true)]
        public bool IncludeTechnical { get; set; } = true;
        
        [Description("Include recommendations")]
        [CommandOption("--recommendations")]
        [DefaultValue(true)]
        public bool IncludeRecommendations { get; set; } = true;
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        try {
            // Show progress
            await AnsiConsole.Progress()
                .AutoClear(false)
                .Columns(new ProgressColumn[] {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn(),
                })
                .StartAsync(async ctx => {
                    // Step 1: Analyze domain
                    var analyzeTask = ctx.AddTask($"[green]Analyzing {settings.Domain}[/]");
                    var healthCheck = new DomainHealthCheck();
                    await healthCheck.Verify(settings.Domain);
                    analyzeTask.Value = 100;
                    
                    // Step 2: Generate report
                    var generateTask = ctx.AddTask("[yellow]Generating report[/]");
                    
                    // Determine output path
                    var outputPath = settings.OutputPath ?? 
                        $"{settings.Domain.Replace(".", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.{settings.Format}";
                    
                    // Generate based on format
                    switch (settings.Format.ToLower()) {
                        case "html":
                            var htmlReport = new DomainSecurityReport(healthCheck, settings.Domain);
                            htmlReport.GenerateReport(outputPath, settings.OpenInBrowser);
                            break;
                            
                        case "word":
                            AnsiConsole.MarkupLine("[red]Word format not yet implemented[/]");
                            return;
                            
                        case "excel":
                            AnsiConsole.MarkupLine("[red]Excel format not yet implemented[/]");
                            return;
                            
                        case "pdf":
                            AnsiConsole.MarkupLine("[red]PDF format not yet implemented[/]");
                            return;
                            
                        default:
                            AnsiConsole.MarkupLine($"[red]Unknown format: {settings.Format}[/]");
                            return;
                    }
                    
                    generateTask.Value = 100;
                    
                    // Show summary
                    AnsiConsole.WriteLine();
                    var panel = new Panel(
                        $"[green]✓[/] Report generated successfully!\n" +
                        $"[blue]Domain:[/] {settings.Domain}\n" +
                        $"[blue]Format:[/] {settings.Format}\n" +
                        $"[blue]Output:[/] {outputPath}\n" +
                        $"[blue]Template:[/] {settings.Template}\n" +
                        $"[blue]Theme:[/] {settings.Theme}"
                    ) {
                        Header = new PanelHeader("Report Generation Complete"),
                        Border = BoxBorder.Rounded
                    };
                    AnsiConsole.Write(panel);
                });
            
            return 0;
        }
        catch (Exception ex) {
            AnsiConsole.MarkupLine($"[red]Error generating report: {ex.Message}[/]");
            return 1;
        }
    }
}