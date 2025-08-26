using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using DomainDetective.CLI.Wizard;
using DomainDetective.Scanning;

namespace DomainDetective.CLI;

/// <summary>Settings for the Hacker Wizard scan.</summary>
internal sealed class WizardScanSettings : CommandSettings
{
    [Description("Domain to scan (e.g. example.com)")]
    [CommandOption("--domain <DOMAIN>")]
    public string Domain { get; set; } = string.Empty;

    [Description("Run a quick scan (skip Web & Reputation)")]
    [CommandOption("--quick")]
    public bool Quick { get; set; }

    [Description("Run a full scan (includes Reputation)")]
    [CommandOption("--full")]
    public bool Full { get; set; }

    [Description("Output format: console|json|html")]
    [CommandOption("--output <FMT>")]
    [DefaultValue("console")]
    public string Output { get; set; } = "console";

    [Description("Path for JSON/HTML export")]
    [CommandOption("--out <PATH>")]
    public string? Out { get; set; }

    [Description("Disable ANSI coloring")]
    [CommandOption("--no-ansi")]
    public bool NoAnsi { get; set; }

    [Description("Matrix theme")]
    [CommandOption("--matrix")]
    public bool Matrix { get; set; }

    [Description("Enable active mail transport probes")]
    [CommandOption("--active-mail-probes")]
    public bool ActiveMailProbes { get; set; }

    [Description("Details level: summary|standard|advanced")]
    [CommandOption("--details <LEVEL>")]
    [DefaultValue("standard")]
    public string Details { get; set; } = "standard";

    [Description("Interactive selection of checks (checkboxes)")]
    [CommandOption("--interactive")]
    public bool Interactive { get; set; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Domain))
            return ValidationResult.Error("--domain is required");
        return ValidationResult.Success();
    }
}

/// <summary>Runs the Hacker Wizard scan with Spectre UI.</summary>
internal sealed class WizardScanCommand : AsyncCommand<WizardScanSettings>
{
    [RequiresDynamicCode("Calls JSON serialization")]
    public override async Task<int> ExecuteAsync(CommandContext context, WizardScanSettings s)
    {
        // Note: Spectre.Console global profile is read-only in this version.
        // We skip toggling ANSI here and rely on environment/terminal settings.
        if (s.Matrix)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
        }

        var mode = s.Full ? DomainDetective.CLI.Wizard.ScanMode.Full : (s.Quick ? DomainDetective.CLI.Wizard.ScanMode.Quick : DomainDetective.CLI.Wizard.ScanMode.Default);

        HealthCheckType[]? selectedChecks = null;
        bool activeProbes = s.ActiveMailProbes;
        string details = s.Details.ToLowerInvariant();

        if (s.Interactive)
        {
            // Let user choose checks via checkboxes with emojis
            var choices = new[]
            {
                "🧭 DNS",
                "📧 Mail",
                "🌐 Web",
                "🛡 Reputation",
                "⚙️ Active mail probes"
            };
            var picked = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title($"[green]Select what to scan for [bold]{s.Domain}[/]:[/]")
                    .InstructionsText("[grey](Press [yellow]<space>[/] to toggle, [yellow]<enter>[/] to accept)[/]")
                    .NotRequired()
                    .PageSize(10)
                    .HighlightStyle(new Style(Color.Green))
                    .AddChoices(choices)
                    .Select("🧭 DNS").Select("📧 Mail")
            );

            var list = new List<HealthCheckType>();
            if (picked.Contains("🧭 DNS")) list.AddRange(new[] { HealthCheckType.NS, HealthCheckType.SOA, HealthCheckType.DNSSEC, HealthCheckType.WILDCARDDNS, HealthCheckType.OPENRESOLVER, HealthCheckType.ZONETRANSFER, HealthCheckType.TTL });
            if (picked.Contains("📧 Mail"))
            {
                list.AddRange(new[] { HealthCheckType.MX, HealthCheckType.SPF, HealthCheckType.DKIM, HealthCheckType.DMARC, HealthCheckType.BIMI, HealthCheckType.MTASTS, HealthCheckType.TLSRPT });
                if (picked.Contains("⚙️ Active mail probes"))
                {
                    activeProbes = true;
                    list.AddRange(new[] { HealthCheckType.STARTTLS, HealthCheckType.SMTPTLS, HealthCheckType.IMAPTLS, HealthCheckType.POP3TLS, HealthCheckType.SMTPBANNER, HealthCheckType.SMTPAUTH, HealthCheckType.OPENRELAY });
                }
            }
            if (picked.Contains("🌐 Web")) list.AddRange(new[] { HealthCheckType.HTTP, HealthCheckType.CERT, HealthCheckType.DANE });
            if (picked.Contains("🛡 Reputation")) list.AddRange(new[] { HealthCheckType.DNSBL, HealthCheckType.RPKI, HealthCheckType.RDAP });
            selectedChecks = list.Distinct().ToArray();

            // Details level quick picker
            details = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Details level")
                    .AddChoices(new[] { "standard", "summary", "advanced" })
                    .HighlightStyle(new Style(Color.Green))
            );
        }

        var wizard = new DomainWizard(new WizardOptions
        {
            Domain = s.Domain.Trim().ToLowerInvariant(),
            Mode = mode,
            Output = s.Output.ToLowerInvariant(),
            Out = s.Out,
            Matrix = s.Matrix,
            ActiveMailProbes = activeProbes,
            Details = details,
            Checks = selectedChecks
        });

        var hc = await wizard.RunAsync(Program.CancellationToken);

        switch (wizard.Options.Output)
        {
            case "json":
                {
                    var json = hc.ToJson();
                    if (!string.IsNullOrWhiteSpace(wizard.Options.Out))
                    {
                        File.WriteAllText(wizard.Options.Out!, json);
                        AnsiConsole.MarkupLine($"[grey]JSON written to[/] [underline]{wizard.Options.Out}[/]");
                    }
                    else
                    {
                        Console.WriteLine(json);
                    }
                    break;
                }
            case "html":
                {
                    AnsiConsole.MarkupLine("[yellow]HTML export is not enabled yet for the wizard.[/]");
                    break;
                }
            default:
                break;
        }

        return 0;
    }
}
