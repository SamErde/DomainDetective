# _

Perfect — so you’re imagining **DomainDetective** starting with just a domain name, and then the wizard kicks off a guided, “hacker-fancy” console session that progressively uncovers layers of DNS, Email, Web, Reputation, Routing, etc.
We want this to **flow like a scan**, not just a dump of tables.

Let’s structure this in two parts:

---

## 1. Wizard Flow (based only on a domain)

Think of it like *stages* the wizard can guide the user through:

### Stage 1 — Domain Prep

* Normalize input (strip scheme, handle `www.` vs apex).
* Resolve to A/AAAA, show PTR/FCrDNS.
* Fetch SOA, NS, TTL analysis.
* Show DNSSEC chain if present.
* Detect wildcard resolution, open resolver, AXFR attempt.
* Show animated “resolving chain…”

### Stage 2 — Mail Checks

* MX fetch + live connectivity check.
* SPF parsing, evaluate pass/fail for `~all`, `-all`.
* DKIM record(s) existence, validate keys.
* DMARC presence, policy strength, RUA/RUF addresses.
* BIMI lookup.
* MTA-STS policy and TLS-RPT ingestion.
* Open relay test (simulation mode, safe).
* Wizard step: ask if user wants to actively connect to MX to verify STARTTLS banners.

### Stage 3 — Web & Certificates

* HTTP/HTTPS connectivity test (status codes, redirects).
* Check HTTP/2, HTTP/3 support.
* Security headers: HSTS, CSP, XFO, etc.
* TLS certificate details: SANs, chain, CT logs.
* DANE/TLSA records.
* S/MIMEA if relevant.

### Stage 4 — Reputation & Intel

* WHOIS / RDAP.
* RPKI origin validation.
* Threat intel feeds (abuse.ch, Spamhaus, PhishTank).
* Blacklist checks.

### Stage 5 — Reports

* Wizard can ask: “Do you want to generate a report?”

  * Options: Console (Matrix-style scan results), JSON, HTML dashboard.
* Background monitoring option: “Would you like to re-run this daily and monitor drift?”

---

## 2. UX Style (Spectre.Console Wizard)

The wizard should feel interactive:

* **Progressive reveal**: show each stage with animation, not dump.
* **Prompt**: occasionally ask: “Do you want deeper analysis (yes/no)?”
* **Modes**: quick scan (DNS + Mail summary) vs full audit (all modules).
* **Output**:

  * Tree view for hierarchy.
  * Panels for PASS/FAIL.
  * Progress bars for percentages (SPF coverage, DMARC alignment).
  * Typewriter effect for banners & connections.
  * Neon/Matrix style color scheme (green on black, with red/yellow for warnings).

---

## 3. Instructions for Codex AI (Implementation Guide)

You’ll want a **task list** Codex can follow for both CLI (C# + Spectre.Console) and PowerShell wrapper.

### High-Level Instructions for Codex AI

```
# DomainDetective Wizard Implementation Instructions

## Goal
Implement an interactive CLI wizard for DomainDetective that accepts a domain and progressively analyzes DNS, Email, Web, Certificates, Reputation, and Registry data.

## CLI Requirements
- Use C# and Spectre.Console for CLI presentation.
- Input: single domain name.
- Flow should be staged (DNS → Email → Web → Reputation → Reports).
- At each stage:
  - Display "scanning" animations (typewriter effect, progress bars).
  - Show results in non-table formats (tree, panels, progress meters).
  - Color scheme: green for pass, red for fail, yellow for warning.
- Provide user prompts (yes/no, select deeper scan).
- Support exporting results to JSON and HTML.

## PowerShell Requirements
- Wrap CLI functions in a PowerShell module.
- Cmdlets should include:
  - `Invoke-DomainDetectiveScan` (quick scan, JSON/HTML output optional)
  - `Start-DomainDetectiveWizard` (interactive wizard mode)
- Parameters:
  - `-Domain`
  - `-Quick` / `-Full`
  - `-OutputFormat JSON|HTML|Console`
- Ensure no extra dependencies except built-in .NET and Spectre.Console.

## Tasks for Codex
1. Create a `Wizard` class in C# using Spectre.Console.
2. Implement staged methods:
   - `RunDnsStage(string domain)`
   - `RunEmailStage(string domain)`
   - `RunWebStage(string domain)`
   - `RunReputationStage(string domain)`
   - `RunReportStage(string domain)`
3. Each stage should:
   - Animate “Scanning...” with delays.
   - Display results in panels/trees.
   - Return structured objects (for JSON/HTML output).
4. Implement PowerShell module wrappers that call the CLI methods.
5. Add JSON/HTML exporters for final reports.

```

---

Would you like me to **write a ready-to-commit `Wizard.cs` skeleton** (Spectre-based, with placeholders for DNS/Email/Web stages), so Codex just has to fill in the logic? That way your AI assistant doesn’t waste time on Spectre boilerplate and focuses on plugging in your existing DomainDetective analyzers.

---

Below is a **drop-in replacement** for the earlier version: it runs **all analyzers in parallel**, uses a **log bus** with typewriter output, shows **Live + Progress** while work happens, and then renders a **deterministic final summary** using **Tree / Panels / BarChart**. It keeps the same public surface and result models, so you can plug in real analyzers where marked.

---

### `DomainDetective.Cli/Program.cs`  *(unchanged except minor tidy)*

```csharp
using System.CommandLine;
using DomainDetective.Cli.Wizard;
using Spectre.Console;

var domainOption = new Option<string>(
    name: "--domain",
    description: "Domain to scan (e.g. example.com)")
{ IsRequired = true };

var quickOption = new Option<bool>("--quick", description: "Run a quick scan (DNS + Mail summary)");
var fullOption  = new Option<bool>("--full", description: "Run a full audit (all modules)");
var outFmt      = new Option<string>("--output", () => "console", "Output format: console|json|html");
var outPath     = new Option<string>("--out", "Path for JSON/HTML export (file or folder).");
var noAnsi      = new Option<bool>("--no-ansi", "Disable ANSI (plain console).");
var darkTheme   = new Option<bool>("--matrix", "Force Matrix/green-on-black theme.");

var root = new RootCommand("DomainDetective — Interactive Wizard");
root.AddOption(domainOption);
root.AddOption(quickOption);
root.AddOption(fullOption);
root.AddOption(outFmt);
root.AddOption(outPath);
root.AddOption(noAnsi);
root.AddOption(darkTheme);

root.SetHandler(async (string domain, bool quick, bool full, string output, string? outFile, bool noColors, bool matrix) =>
{
    AnsiConsole.Profile = new Profile(capabilities: new Capabilities(emojis: true, links: true, ansi: !noColors));
    if (matrix)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear();
    }

    var wizard = new DomainWizard(new WizardOptions
    {
        Domain = domain.Trim().ToLowerInvariant(),
        Mode   = full ? ScanMode.Full : (quick ? ScanMode.Quick : ScanMode.Default),
        Output = output.ToLowerInvariant(),     // console|json|html
        Out    = outFile,
        Matrix = matrix
    });

    var result = await wizard.RunAsync();

    // Export (if requested)
    switch (wizard.Options.Output)
    {
        case "json":
            {
                var json = Exporters.JsonExporter.Serialize(result);
                if (!string.IsNullOrWhiteSpace(wizard.Options.Out))
                {
                    System.IO.File.WriteAllText(wizard.Options.Out!, json);
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
                var html = Exporters.HtmlExporter.Render(result, wizard.Options);
                if (!string.IsNullOrWhiteSpace(wizard.Options.Out))
                {
                    System.IO.File.WriteAllText(wizard.Options.Out!, html);
                    AnsiConsole.MarkupLine($"[grey]HTML written to[/] [underline]{wizard.Options.Out}[/]");
                }
                else
                {
                    Console.WriteLine(html);
                }
                break;
            }
        default:
            break; // console already rendered
    }

}, domainOption, quickOption, fullOption, outFmt, outPath, noAnsi, darkTheme);

return await root.InvokeAsync(args);
```

---

### `DomainDetective.Cli/Wizard/Wizard.cs`  *(parallel orchestration + Live + final render)*

```csharp
using Spectre.Console;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace DomainDetective.Cli.Wizard;

// ---------- Options & Modes ----------
public enum ScanMode { Default, Quick, Full }

public sealed class WizardOptions
{
    public required string Domain { get; init; }
    public ScanMode Mode { get; init; } = ScanMode.Default;
    public string Output { get; init; } = "console"; // console|json|html
    public string? Out { get; init; }
    public bool Matrix { get; init; } = false;
}

// ---------- Result Models ----------
public sealed class ScanResult
{
    public string Domain { get; init; } = "";
    public DateTime StartedUtc { get; init; } = DateTime.UtcNow;
    public DateTime FinishedUtc { get; set; }

    public DnsResult Dns { get; init; } = new();
    public MailResult Mail { get; init; } = new();
    public WebResult Web { get; init; } = new();
    public ReputationResult Reputation { get; init; } = new();

    public List<string> Notes { get; } = new();
}

public sealed class DnsResult
{
    public SoaInfo? Soa { get; set; }
    public List<string> Ns { get; set; } = new();
    public List<MxInfo> Mx { get; set; } = new();
    public bool? DnssecEnabled { get; set; }
    public bool? ZoneTransferOpen { get; set; }
    public bool? WildcardDetected { get; set; }
    public bool? OpenResolver { get; set; }
    public TimeSpan? MinTtl { get; set; }
    public TimeSpan? MaxTtl { get; set; }
    public List<PTRInfo> Reverse { get; set; } = new();
    public bool? FCrDnsAligned { get; set; }
}

public sealed class MailResult
{
    public string? SpfRecord { get; set; }
    public string? DmarcRecord { get; set; }
    public string? DkimSelectorHint { get; set; }
    public Dictionary<string, bool?> DkimSelectorsOk { get; set; } = new();
    public string? BimiRecord { get; set; }
    public string? MtaStsPolicy { get; set; }
    public string? TlsRpt { get; set; }

    public bool? SmtpStartTlsOk { get; set; }
    public bool? ImapTlsOk { get; set; }
    public bool? Pop3TlsOk { get; set; }
    public bool? OpenRelaySuspected { get; set; }

    public MailPolicyScore PolicyScore { get; set; } = new(); // drives bars
}

public sealed class WebResult
{
    public bool? HttpOk { get; set; }
    public bool? HttpsOk { get; set; }
    public bool? Http2 { get; set; }
    public bool? Http3 { get; set; }
    public bool? Hsts { get; set; }
    public List<string> SecurityHeadersMissing { get; set; } = new();
    public TlsChainInfo? Tls { get; set; }
    public bool? DaneTlsa { get; set; }
    public bool? SMIMEA { get; set; }
}

public sealed class ReputationResult
{
    public string? WhoisRegistrar { get; set; }
    public string? RdapHandle { get; set; }
    public bool? RpkiValid { get; set; }
    public List<string> Blacklists { get; set; } = new();
}

// ---------- Submodels ----------
public sealed class SoaInfo { public string? PrimaryNs { get; set; } public string? RName { get; set; } public long? Serial { get; set; } }
public sealed class MxInfo { public string Host { get; set; } = ""; public int Preference { get; set; } public bool? Resolvable { get; set; } }
public sealed class PTRInfo { public string Ip { get; set; } = ""; public string? Ptr { get; set; } }
public sealed class TlsChainInfo { public string? Subject { get; set; } public string? Issuer { get; set; } public DateTimeOffset? NotAfter { get; set; } }
public sealed class MailPolicyScore
{
    public int SpfCoverage { get; set; }
    public int DkimSelectors { get; set; }
    public int DmarcStrength { get; set; }
    public int TransportTlsPosture { get; set; }
}

// ---------- Log bus (producer: analyzers, consumer: UI) ----------
file static class FxLog
{
    private static readonly Channel<string> _bus = Channel.CreateUnbounded<string>();

    public static void Enqueue(string markup) => _bus.Writer.TryWrite(markup);

    public static async Task ConsumeAsync(CancellationToken ct)
    {
        await foreach (var line in _bus.Reader.ReadAllAsync(ct))
            await Fx.TypeLineAsync(line);
    }

    public static void Complete() => _bus.Writer.TryComplete();
}

// ---------- Wizard ----------
public sealed class DomainWizard
{
    public WizardOptions Options { get; }
    public DomainWizard(WizardOptions options) => Options = options;

    public async Task<ScanResult> RunAsync()
    {
        var res = new ScanResult { Domain = Options.Domain };

        if (Options.Output == "console")
            Fx.TitleScreen(Options.Domain, Options.Matrix);

        // Kick off the typewriter log consumer
        using var cts = new CancellationTokenSource();
        var logConsumer = FxLog.ConsumeAsync(cts.Token);

        // Prepare parallel analyzers: fill DTOs, enqueue logs, no direct UI writes
        var tasks = new List<Func<Task>>
        {
            () => AnalyzeDnsAsync(res),
            () => AnalyzeMailAsync(res),
            () => AnalyzeWebMaybeAsync(res),
            () => AnalyzeReputationMaybeAsync(res)
        };

        // Live + Progress surface during execution
        await Fx.RunParallelWithLiveAsync(tasks);

        // Stop the log consumer
        FxLog.Complete();
        cts.Cancel();
        try { await logConsumer; } catch { /* ignore */ }

        res.FinishedUtc = DateTime.UtcNow;

        // Deterministic, single-threaded final render
        if (Options.Output == "console")
        {
            Fx.FinalSummary(res);
            Ui.RenderDnsTree(res.Dns, res.Domain);
            Ui.RenderMailSummary(res.Mail);
            Ui.RenderWebSummary(res.Web);
            Ui.RenderReputationSummary(res.Reputation);
        }

        return res;
    }

    // ---------- Analyzer placeholders (replace TODOs with real logic) ----------
    private async Task AnalyzeDnsAsync(ScanResult result)
    {
        FxLog.Enqueue("[grey]> DNS: discovering NS/SOA/MX/DNSSEC/TTL…[/]");
        await Task.Delay(200); // simulate latency

        // TODO: Replace with real DNS lookups
        result.Dns.Ns = new() { $"ns1.{result.Domain}", $"ns2.{result.Domain}" };
        result.Dns.Soa = new SoaInfo { PrimaryNs = $"ns1.{result.Domain}", RName = "hostmaster." + result.Domain, Serial = 2025082501 };

        result.Dns.Mx = new()
        {
            new MxInfo { Host = $"mx1.{result.Domain}", Preference = 10, Resolvable = true },
            new MxInfo { Host = $"mx2.{result.Domain}", Preference = 20, Resolvable = true },
        };

        result.Dns.DnssecEnabled    = true;
        result.Dns.WildcardDetected = false;
        result.Dns.OpenResolver     = false;
        result.Dns.ZoneTransferOpen = false;
        result.Dns.MinTtl = TimeSpan.FromMinutes(5);
        result.Dns.MaxTtl = TimeSpan.FromHours(24);

        FxLog.Enqueue("[green]✓[/] DNS phase complete");
    }

    private async Task AnalyzeMailAsync(ScanResult result)
    {
        FxLog.Enqueue("[grey]> Mail: SPF/DKIM/DMARC/BIMI/MTA-STS/TLS-RPT…[/]");
        await Task.Delay(200);

        // TODO: TXT lookups + evaluation
        result.Mail.SpfRecord   = $"v=spf1 include:_spf.{result.Domain} -all";
        result.Mail.DmarcRecord = "v=DMARC1; p=reject; rua=mailto:dmarc@" + result.Domain;
        result.Mail.BimiRecord  = "v=BIMI1; l=https://example.com/logo.svg; a=self";
        result.Mail.MtaStsPolicy = "mode=enforce; mx: mx1." + result.Domain;
        result.Mail.TlsRpt      = "v=TLSRPTv1; rua=mailto:tlsrpt@" + result.Domain;

        result.Mail.DkimSelectorHint = "Try: default, selector1, google";
        result.Mail.DkimSelectorsOk["default"]  = true;
        result.Mail.DkimSelectorsOk["selector1"] = true;
        result.Mail.DkimSelectorsOk["google"]   = null;

        result.Mail.PolicyScore = new MailPolicyScore
        {
            SpfCoverage = 90,
            DkimSelectors = 75,
            DmarcStrength = 100,
            TransportTlsPosture = 80
        };

        // Optional transport probes could run here (gated by a setting/flag)

        FxLog.Enqueue("[green]✓[/] Mail phase complete");
    }

    private async Task AnalyzeWebMaybeAsync(ScanResult result)
    {
        if (Options.Mode == ScanMode.Quick)
        {
            FxLog.Enqueue("[grey]> Web: skipped in Quick mode[/]");
            return;
        }

        FxLog.Enqueue("[grey]> Web: HTTP/HTTPS/H2/H3/HSTS/SecHeaders/TLS…[/]");
        await Task.Delay(200);

        // TODO: real HTTP/TLS checks
        result.Web.HttpOk  = true;
        result.Web.HttpsOk = true;
        result.Web.Http2   = true;
        result.Web.Http3   = false;
        result.Web.Hsts    = true;
        result.Web.SecurityHeadersMissing = new(); // add if any

        result.Web.Tls = new TlsChainInfo
        {
            Subject = $"CN={result.Domain}",
            Issuer = "R3 (Let’s Encrypt)",
            NotAfter = DateTimeOffset.UtcNow.AddDays(60)
        };

        result.Web.DaneTlsa = false;
        result.Web.SMIMEA   = false;

        FxLog.Enqueue("[green]✓[/] Web phase complete");
    }

    private async Task AnalyzeReputationMaybeAsync(ScanResult result)
    {
        if (Options.Mode != ScanMode.Full)
        {
            FxLog.Enqueue("[grey]> Reputation: skipped (use --full)[/]");
            return;
        }

        FxLog.Enqueue("[grey]> Reputation: WHOIS/RDAP/RPKI/Feeds…[/]");
        await Task.Delay(200);

        // TODO: WHOIS/RDAP/RPKI + feeds
        result.Reputation.WhoisRegistrar = "Example Registrar LLC";
        result.Reputation.RdapHandle     = "RDAP-XYZ-123";
        result.Reputation.RpkiValid      = true;
        result.Reputation.Blacklists     = new();

        FxLog.Enqueue("[green]✓[/] Reputation phase complete");
    }
}

// ---------- UI Runtime (Live + Progress + Typewriter + Final) ----------
file static class Fx
{
    // Live model with progress tasks
    private sealed class LiveModel
    {
        public ProgressTask? Dns;
        public ProgressTask? Mail;
        public ProgressTask? Web;
        public ProgressTask? Rep;
    }

    public static void TitleScreen(string domain, bool matrix)
    {
        var rule = new Rule($"[bold green]DOMAINDETECTIVE[/] [grey]// Wizard[/]") { Alignment = Justify.Center };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"[green]Target:[/] [bold]{domain}[/]");
        if (matrix) AnsiConsole.MarkupLine("[grey]Theme:[/] [green]Matrix[/]");
        AnsiConsole.WriteLine();
    }

    public static async Task TypeLineAsync(string markup, int delayMs = 8)
    {
        foreach (var ch in markup)
        {
            AnsiConsole.Markup(ch.ToString());
            await Task.Delay(delayMs);
        }
        AnsiConsole.WriteLine();
    }

    // Run all analyzers in parallel and show live progress UI
    public static async Task RunParallelWithLiveAsync(IReadOnlyList<Func<Task>> analyzers)
    {
        var liveModel = new LiveModel();

        await AnsiConsole.Live(new Rows()).StartAsync(async ctx =>
        {
            // Build a live progress surface
            var progress = new Progress()
                .AutoClear(false)
                .HideCompleted(false)
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn()
                });

            // Show the progress in the live region
            ctx.UpdateTarget(progress);

            await AnsiConsole.Progress()
                .AutoClear(false)
                .StartAsync(async prog =>
                {
                    liveModel.Dns  = prog.AddTask("DNS", autoStart: false, maxValue: 100);
                    liveModel.Mail = prog.AddTask("Mail", autoStart: false, maxValue: 100);
                    liveModel.Web  = prog.AddTask("Web", autoStart: false, maxValue: 100);
                    liveModel.Rep  = prog.AddTask("Reputation", autoStart: false, maxValue: 100);

                    // Map analyzers → progress tasks
                    var runners = new List<Task>
                    {
                        Task.Run(async () => { liveModel.Dns!.StartTask();  await analyzers[0](); liveModel.Dns.Value  = 100; }),
                        Task.Run(async () => { liveModel.Mail!.StartTask(); await analyzers[1](); liveModel.Mail.Value = 100; }),
                        Task.Run(async () => { liveModel.Web!.StartTask();  await analyzers[2](); liveModel.Web.Value  = 100; }),
                        Task.Run(async () => { liveModel.Rep!.StartTask();  await analyzers[3](); liveModel.Rep.Value  = 100; }),
                    };

                    await Task.WhenAll(runners);
                });

            // Replace progress with a simple “Completed” marker. Final rich render happens outside Live.
            ctx.UpdateTarget(new Panel(new Markup("[green]All stages completed[/]")) { Border = BoxBorder.Rounded, Header = new PanelHeader("[bold]Scan[/]") });
        });
    }

    public static void FinalSummary(ScanResult res)
    {
        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow(new Markup("[bold]Domain[/]"), new Markup(res.Domain));
        grid.AddRow(new Markup("[bold]Started[/]"), new Markup(res.StartedUtc.ToString("u")));
        grid.AddRow(new Markup("[bold]Finished[/]"), new Markup(res.FinishedUtc.ToString("u")));

        var p = new Panel(grid) { Header = new PanelHeader("[bold]Scan Complete[/]"), Border = BoxBorder.Heavy };
        AnsiConsole.Write(p);
        AnsiConsole.WriteLine();
    }

    public static string Bool(bool? v) => v == true ? "[green]OK[/]" : v == false ? "[red]NO[/]" : "[yellow]?[/]";

    public static Panel PostureBars(MailPolicyScore s)
    {
        var bars = new BarChart().Width(36);
        bars.AddItem("SPF",  s.SpfCoverage,        s.SpfCoverage       >= 85 ? Color.Green : s.SpfCoverage       >= 60 ? Color.Yellow : Color.Red);
        bars.AddItem("DKIM", s.DkimSelectors,      s.DkimSelectors     >= 85 ? Color.Green : s.DkimSelectors     >= 60 ? Color.Yellow : Color.Red);
        bars.AddItem("DMARC",s.DmarcStrength,      s.DmarcStrength     >= 85 ? Color.Green : s.DmarcStrength     >= 60 ? Color.Yellow : Color.Red);
        bars.AddItem("TLS",  s.TransportTlsPosture,s.TransportTlsPosture>=85 ? Color.Green : s.TransportTlsPosture>=60 ? Color.Yellow : Color.Red);
        return new Panel(bars) { Header = new PanelHeader("[bold]Email Posture[/]"), Border = BoxBorder.Rounded };
    }
}

// ---------- Final Renderers (Tree / Panels / Bars) ----------
file static class Ui
{
    public static void RenderDnsTree(DnsResult dns, string domain)
    {
        var root = new Tree($"[bold]DNS for {domain}[/]");

        var ns = root.AddNode("[white]NS[/]");
        foreach (var n in dns.Ns) ns.AddNode($"[green]{n.EscapeMarkup()}[/]");

        var soa = root.AddNode("[white]SOA[/]");
        if (dns.Soa is not null)
        {
            soa.AddNode($"MNAME: [yellow]{dns.Soa.PrimaryNs?.EscapeMarkup()}[/]");
            soa.AddNode($"RNAME: [yellow]{dns.Soa.RName?.EscapeMarkup()}[/]");
            soa.AddNode($"Serial: [yellow]{dns.Soa.Serial}[/]");
        }

        var mx = root.AddNode("[white]MX[/]");
        foreach (var m in dns.Mx.OrderBy(x => x.Preference))
            mx.AddNode($"{m.Preference} [green]{m.Host.EscapeMarkup()}[/] {(m.Resolvable==true?"[green]OK[/]":"[red]NX[/]")}");

        var posture = new Panel(new Rows(
            new Markup($"DNSSEC: {Fx.Bool(dns.DnssecEnabled)}"),
            new Markup($"Wildcard: {Fx.Bool(!(dns.WildcardDetected==true))}"),
            new Markup($"Open Resolver: {Fx.Bool(!(dns.OpenResolver==true))}"),
            new Markup($"AXFR: {Fx.Bool(!(dns.ZoneTransferOpen==true))}"),
            new Markup($"TTL(min): {dns.MinTtl?.ToString()??"—"}"),
            new Markup($"TTL(max): {dns.MaxTtl?.ToString()??"—"}")
        ))
        { Header = new PanelHeader("[bold]DNS Posture[/]"), Border = BoxBorder.Rounded };

        var layout = new Grid().AddColumn().AddColumn();
        layout.AddRow(root, posture);

        AnsiConsole.Write(layout);
        AnsiConsole.WriteLine();
    }

    public static void RenderMailSummary(MailResult m)
    {
        var tree = new Tree("[bold]Email[/]");
        tree.AddNode("[white]SPF[/]").AddNode((m.SpfRecord ?? "—").EscapeMarkup());
        tree.AddNode("[white]DMARC[/]").AddNode((m.DmarcRecord ?? "—").EscapeMarkup());

        var dkim = tree.AddNode("[white]DKIM[/]");
        foreach (var kv in m.DkimSelectorsOk)
            dkim.AddNode($"{kv.Key}: {(kv.Value==true?"[green]OK[/]": kv.Value==false?"[red]FAIL[/]":"[yellow]?[/]")}");

        var details = new Panel(new Rows(
            new Markup($"BIMI: {(m.BimiRecord ?? "—").EscapeMarkup()}"),
            new Markup($"MTA-STS: {(m.MtaStsPolicy ?? "—").EscapeMarkup()}"),
            new Markup($"TLS-RPT: {(m.TlsRpt ?? "—").EscapeMarkup()}"),
            new Markup($"Hint: {(m.DkimSelectorHint ?? "—").EscapeMarkup()}")
        )) { Header = new PanelHeader("[bold]Email Details[/]"), Border = BoxBorder.Rounded };

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow(tree, Fx.PostureBars(m.PolicyScore));

        AnsiConsole.Write(grid);
        AnsiConsole.Write(details);
        AnsiConsole.WriteLine();
    }

    public static void RenderWebSummary(WebResult web)
    {
        // KV panel (could be split into tree + posture later)
        var kv = new Dictionary<string, string?>
        {
            ["HTTP"]     = Fx.Bool(web.HttpOk),
            ["HTTPS"]    = Fx.Bool(web.HttpsOk),
            ["HTTP/2"]   = Fx.Bool(web.Http2),
            ["HTTP/3"]   = Fx.Bool(web.Http3),
            ["HSTS"]     = Fx.Bool(web.Hsts),
            ["Missing"]  = web.SecurityHeadersMissing.Count == 0 ? "none" : string.Join(", ", web.SecurityHeadersMissing),
            ["TLS.Subject"] = web.Tls?.Subject,
            ["TLS.Issuer"]  = web.Tls?.Issuer,
            ["TLS.NotAfter"]= web.Tls?.NotAfter?.ToString("u"),
            ["DANE/TLSA"]   = Fx.Bool(web.DaneTlsa),
            ["S/MIMEA"]     = Fx.Bool(web.SMIMEA),
        };

        var rows = new List<IRenderable>();
        foreach (var p in kv)
            rows.Add(new Markup($"[grey]{p.Key}:[/] {p.Value?.EscapeMarkup() ?? "—"}"));

        var panel = new Panel(new Rows(rows)) { Header = new PanelHeader("[bold]Web[/]"), Border = BoxBorder.Rounded };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public static void RenderReputationSummary(ReputationResult rep)
    {
        var rows = new Rows(
            new Markup($"Registrar: {(rep.WhoisRegistrar ?? "—").EscapeMarkup()}"),
            new Markup($"RDAP: {(rep.RdapHandle ?? "—").EscapeMarkup()}"),
            new Markup($"RPKI: {Fx.Bool(rep.RpkiValid)}"),
            new Markup($"Blacklists: {(rep.Blacklists.Count==0 ? "none" : string.Join(", ", rep.Blacklists).EscapeMarkup())}")
        );
        var panel = new Panel(rows) { Header = new PanelHeader("[bold]Reputation & Registry[/]"), Border = BoxBorder.Rounded };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }
}

// ---------- Exporters ----------
namespace Exporters
{
    public static class JsonExporter
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(ScanResult res) => JsonSerializer.Serialize(res, _opts);
    }

    public static class HtmlExporter
    {
        public static string Render(ScanResult res, WizardOptions options)
        {
            // Minimal, replace with HtmlForgeX
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<!doctype html><html><head><meta charset='utf-8'><title>DomainDetective Report</title>");
            sb.AppendLine("<style>body{font-family:ui-sans-serif,system-ui;background:#0b0f10;color:#e5f2e5} .ok{color:#6ee7a8}.bad{color:#f87171}.warn{color:#fbbf24} .card{border:1px solid #1f2937;padding:12px;border-radius:8px;margin:10px;background:#111827}</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine($"<h1>DomainDetective — {res.Domain}</h1>");
            sb.AppendLine("<div class='card'><h2>DNS</h2>");
            sb.Append("<ul>");
            foreach (var ns in res.Dns.Ns) sb.Append($"<li>NS: {ns}</li>");
            foreach (var mx in res.Dns.Mx.OrderBy(m => m.Preference)) sb.Append($"<li>MX: {mx.Preference} {mx.Host} {(mx.Resolvable==true?"✅":"❌")}</li>");
            sb.Append($"<li>DNSSEC: {(res.Dns.DnssecEnabled==true?"✅":"❌")}</li>");
            sb.Append($"<li>AXFR: {(res.Dns.ZoneTransferOpen==true?"❌ open":"✅ closed")}</li>");
            sb.Append("</ul></div>");

            sb.AppendLine("<div class='card'><h2>Email</h2><ul>");
            sb.Append($"<li>SPF: {res.Mail.SpfRecord}</li>");
            sb.Append($"<li>DMARC: {res.Mail.DmarcRecord}</li>");
            sb.Append($"<li>BIMI: {res.Mail.BimiRecord}</li>");
            sb.Append($"<li>MTA-STS: {res.Mail.MtaStsPolicy}</li>");
            sb.Append($"<li>TLS-RPT: {res.Mail.TlsRpt}</li>");
            sb.Append("</ul></div>");

            sb.AppendLine("<div class='card'><h2>Web</h2><ul>");
            sb.Append($"<li>HTTP: {(res.Web.HttpOk==true?"✅":"❌")} / HTTPS: {(res.Web.HttpsOk==true?"✅":"❌")}</li>");
            sb.Append($"<li>H2: {(res.Web.Http2==true?"✅":"❌")} / H3: {(res.Web.Http3==true?"✅":"❌")}</li>");
            sb.Append($"<li>HSTS: {(res.Web.Hsts==true?"✅":"❌")}</li>");
            if (res.Web.Tls is not null)
                sb.Append($"<li>TLS: {res.Web.Tls.Subject} / {res.Web.Tls.Issuer} / NotAfter {res.Web.Tls.NotAfter:u}</li>");
            sb.Append("</ul></div>");

            sb.AppendLine("<div class='card'><h2>Reputation</h2><ul>");
            sb.Append($"<li>Registrar: {res.Reputation.WhoisRegistrar}</li>");
            sb.Append($"<li>RDAP: {res.Reputation.RdapHandle}</li>");
            sb.Append($"<li>RPKI: {(res.Reputation.RpkiValid==true?"✅":"❌")}</li>");
            if (res.Reputation.Blacklists.Count>0)
                sb.Append($"<li>Blacklists: {string.Join(", ", res.Reputation.Blacklists)}</li>");
            sb.Append("</ul></div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }
    }
}
```

---

### How Codex should plug analyzers

* Implement real logic inside:

  * `AnalyzeDnsAsync`
  * `AnalyzeMailAsync`
  * `AnalyzeWebMaybeAsync`
  * `AnalyzeReputationMaybeAsync`
* Keep **all UI calls out** of analyzers. Only fill `ScanResult` and `FxLog.Enqueue(...)` lines.
* If you add long sub-steps: `FxLog.Enqueue("[grey]> DNSSEC: validating DS chain…[/]");`

---

### Build & Run

```
dotnet add package Spectre.Console
dotnet add package System.CommandLine --prerelease
dotnet build -c Release
./bin/Release/net8.0/DomainDetective.Cli.exe --domain example.com --full --matrix
```

PowerShell wrappers from the previous message continue to work unchanged.


```

---

### `PowerShell/DomainDetective.psm1`

```powershell
# Requires -Version 5.1
# Assumes the CLI was published alongside the module, or provide -Path
# Exposes two commands:
#   Start-DomainDetectiveWizard (interactive console)
#   Invoke-DomainDetectiveScan  (non-interactive, returns objects or writes JSON/HTML)

function Get-DomainDetectiveCliPath {
    param(
        [string] $Path
    )
    if ($Path) { return $Path }
    $moduleRoot = Split-Path -Parent $PSCommandPath
    $candidate  = Join-Path $moduleRoot "DomainDetective.Cli.exe"
    if (Test-Path $candidate) { return $candidate }
    throw "DomainDetective.Cli.exe not found. Provide -Path."
}

function Start-DomainDetectiveWizard {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $Domain,
        [switch] $Quick,
        [switch] $Full,
        [switch] $Matrix,
        [string] $Path
    )
    $exe = Get-DomainDetectiveCliPath -Path $Path
    $args = @("--domain", $Domain, "--output", "console")
    if ($Quick)  { $args += "--quick" }
    if ($Full)   { $args += "--full" }
    if ($Matrix) { $args += "--matrix" }

    & $exe @args
}

function Invoke-DomainDetectiveScan {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $Domain,
        [ValidateSet("console","json","html")] [string] $Output = "json",
        [string] $Out,
        [switch] $Quick,
        [switch] $Full,
        [string] $Path
    )
    $exe = Get-DomainDetectiveCliPath -Path $Path
    $args = @("--domain", $Domain, "--output", $Output)
    if ($Out)  { $args += @("--out", $Out) }
    if ($Quick) { $args += "--quick" }
    if ($Full)  { $args += "--full" }

    if ($Output -eq "json" -and -not $Out) {
        # Capture JSON to object and emit on pipeline
        $json = & $exe @args | Out-String
        if (-not $json) { return }
        $obj = $json | ConvertFrom-Json -Depth 6
        return $obj
    } else {
        & $exe @args
    }
}
Export-ModuleMember -Function Start-DomainDetectiveWizard, Invoke-DomainDetectiveScan
```

---

## How to wire in your analyzers (for Codex to follow)

* Replace the `TODO` blocks inside each stage with your real logic:

  * **DNS**: your resolver pipeline for NS/SOA/MX/DS/DNSSEC chain, wildcard, TTLs, open resolver, AXFR.
  * **Mail**: SPF/DKIM/DMARC/BIMI/MTA-STS/TLS-RPT TXT retrieval & evaluation; optional live SMTP/IMAP/POP3 STARTTLS probes.
  * **Web**: HTTP/HTTPS check, HTTP/2/3 detection, HSTS + security headers audit, TLS chain parse, DANE/TLSA, S/MIMEA.
  * **Reputation**: WHOIS/RDAP parsers, RPKI validation, threat feeds.
* Keep data in `ScanResult` so JSON/HTML export stays automatic.
* The Spectre helpers (`Fx`) already give you:

  * Typewriter lines
  * Spinners
  * Panels with pass/fail coloring
  * Progress bars for “posture” metrics
  * Tree displays for lists (NS, MX, etc.)
* For a **full Matrix vibe**, run with `--matrix`.

---

## Build & Run

```
dotnet new console -n DomainDetective.Cli
# Replace Program.cs with the one above and add Wizard/Wizard.cs
dotnet add package Spectre.Console
dotnet add package System.CommandLine --prerelease
dotnet build -c Release
./bin/Release/net8.0/DomainDetective.Cli.exe --domain example.com --full --matrix
```

PowerShell:

```
# Place DomainDetective.psm1 next to the built exe
Import-Module ./PowerShell/DomainDetective.psm1
Start-DomainDetectiveWizard -Domain example.com -Full -Matrix
Invoke-DomainDetectiveScan -Domain example.com -Output json
```

---
Here’s a compact **Spectre UI cookbook** tailored for DomainDetective—with exact widgets to use, when to use them, and **concurrency-safe patterns** so you can compute in parallel but render cleanly without output collisions.

---

# What to use (and where)

* **Animated typewriter** → for “scan log” feel (start/end of each stage, banners, warnings).
* **Tree / Branch** → hierarchical data (DNS → NS/MX/TXT; Mail → SPF/DKIM/DMARC; Web → Protocols/TLS/SecHeaders).
* **Panels** → posture summaries (DNS Posture, Email Posture, Web Posture, Registry/Routing).
* **BarChart** → posture “scores” (SPF coverage, DKIM selectors verified, DMARC strength, TLS posture).
* **HeatMap** (or Canvas fallback) → TTL distribution, DNS propagation across resolvers/regions, header completeness matrix.
* **Progress** (multi-task) → live, parallel “work in progress” indicators per stage.
* **Live** sections → placeholders you update as results arrive (avoids interleaving from background tasks).
* **Grid / Columns** → arrange multiple panels cleanly.

---

# Golden rule for parallel UI

* **Do work in parallel**; **render on one thread**.
* Use `Task.WhenAll` for analyzers; push interim messages to a **channel/queue**; the UI thread reads and displays them.
* For live widgets, wrap them in `AnsiConsole.Live(...)` and only mutate Spectre objects **inside** the live callback.

---

## 1) Typewriter + log bus (concurrency-safe)

```csharp
// FxLog.cs
using System.Threading.Channels;
using Spectre.Console;

public static class FxLog
{
    private static readonly Channel<string> _bus = Channel.CreateUnbounded<string>();

    public static void Enqueue(string markup) => _bus.Writer.TryWrite(markup);

    public static async Task ConsumeAsync(CancellationToken ct)
    {
        await foreach (var line in _bus.Reader.ReadAllAsync(ct))
            await Fx.TypeLineAsync(line); // your typewriter
    }
}

// usage in Program/Wizard:
var cts = new CancellationTokenSource();
var logConsumer = FxLog.ConsumeAsync(cts.Token);

// anywhere in analyzers (background threads):
FxLog.Enqueue("[grey]> Resolving NS…[/]");
FxLog.Enqueue("[green]✓[/] ns1.example.com");

// when finished:
cts.Cancel();
await logConsumer;
```

Typewriter (from your earlier code) stays the same. Only the **consumer** prints, so no interleaving.

---

## 2) Tree/Branch for hierarchical results

```csharp
public static class Ui
{
    public static void DnsTree(DnsResult dns, string domain)
    {
        var root = new Tree($"[bold]DNS for {domain}[/]");
        var ns = root.AddNode("[white]NS[/]");
        foreach (var n in dns.Ns)
            ns.AddNode($"[green]{n}[/]");

        var soa = root.AddNode("[white]SOA[/]");
        if (dns.Soa is not null)
        {
            soa.AddNode($"MNAME: [yellow]{dns.Soa.PrimaryNs}[/]");
            soa.AddNode($"RNAME: [yellow]{dns.Soa.RName}[/]");
            soa.AddNode($"Serial: [yellow]{dns.Soa.Serial}[/]");
        }

        var mx = root.AddNode("[white]MX[/]");
        foreach (var m in dns.Mx.OrderBy(x => x.Preference))
            mx.AddNode($"{m.Preference} [green]{m.Host}[/] {(m.Resolvable==true?"[green]OK[/]":"[red]NX[/]")}");

        var txt = root.AddNode("[white]TXT[/]");
        // add SPF/DMARC etc. here if you want them under DNS

        AnsiConsole.Write(root);
    }
}
```

Use trees prominently at the **end of each stage** to summarize structure.

---

## 3) Panels + BarChart “posture meters”

```csharp
public static class UiBars
{
    public static Panel PostureBars(MailPolicyScore s)
    {
        var bars = new BarChart().Width(36);
        bars.AddItem("SPF", s.SpfCoverage, s.SpfCoverage>=85?Color.Green: s.SpfCoverage>=60?Color.Yellow:Color.Red);
        bars.AddItem("DKIM", s.DkimSelectors, s.DkimSelectors>=85?Color.Green: s.DkimSelectors>=60?Color.Yellow:Color.Red);
        bars.AddItem("DMARC", s.DmarcStrength, s.DmarcStrength>=85?Color.Green: s.DmarcStrength>=60?Color.Yellow:Color.Red);
        bars.AddItem("TLS", s.TransportTlsPosture, s.TransportTlsPosture>=85?Color.Green: s.TransportTlsPosture>=60?Color.Yellow:Color.Red);

        return new Panel(bars) { Header = new PanelHeader("[bold]Email Posture[/]"), Border = BoxBorder.Rounded };
    }
}
```

Put these bars **next to** a details panel in a `Grid`:

```csharp
var grid = new Grid().AddColumn().AddColumn();
grid.AddRow(
   new Panel(new Markup($"[grey]SPF:[/] {mail.SpfRecord}\n[grey]DMARC:[/] {mail.DmarcRecord}\n[grey]BIMI:[/] {mail.BimiRecord}")) { Header=new PanelHeader("Email") },
   UiBars.PostureBars(mail.PolicyScore)
);
AnsiConsole.Write(grid);
```

---

## 4) HeatMap / matrix-style visual

Spectre has `HeatMap`. If you prefer a custom look, use `Canvas`. Two use cases:

* **TTL heatmap** (bins of TTL values)
* **DNS propagation** (rows: resolvers/regions; cols: status)

```csharp
// HeatMap of TTL buckets
using Spectre.Console;

public static void TtlHeatmap(Dictionary<string,int> bucketToCount)
{
    var hm = new HeatMap();
    foreach (var kv in bucketToCount)
        hm.Add(kv.Key, kv.Value); // Spectre will scale colors

    var panel = new Panel(hm) { Header = new("[bold]TTL Distribution[/]") };
    AnsiConsole.Write(panel);
}
```

**Fallback via Canvas** (if you want a pixel grid with legends):

```csharp
public static void PropagationCanvas(bool[,] ok, string[] regions)
{
    var h = ok.GetLength(0);
    var w = ok.GetLength(1);
    var canvas = new Canvas(w, h);
    for (int y=0; y<h; y++)
      for (int x=0; x<w; x++)
        canvas.SetPixel(x, y, ok[y,x] ? Color.Green : Color.Red);

    var rows = new Rows(
        new Markup("[bold]DNS Propagation[/]"),
        canvas,
        new Markup("[grey]Rows = regions; Cols = resolvers/timepoints[/]")
    );
    AnsiConsole.Write(new Panel(rows){Border=BoxBorder.Rounded});
}
```

---

## 5) Live + Progress: run analyzers in parallel, render safely

Use `AnsiConsole.Live` with shared **view-models** you mutate as tasks finish.

```csharp
public sealed class LiveModel
{
    public ProgressTask? DnsTask, MailTask, WebTask, RepTask;
    public Tree? DnsTree, MailTree, WebTree, RepTree;
}

public static async Task RunParallelWithLiveAsync(Func<Task>[] analyzers, Action<LiveModel> buildStaticSections)
{
    var model = new LiveModel();
    await AnsiConsole.Live(new Rows()).StartAsync(async ctx =>
    {
        // Build static shell
        buildStaticSections(model);

        // Progress
        var progress = new Progress()
            .AutoClear(false)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
            });

        // Attach to Live
        ctx.UpdateTarget(new Rows(progress));

        await AnsiConsole.Progress()
            .AutoClear(false)
            .StartAsync(async prog =>
            {
                model.DnsTask  = prog.AddTask("DNS",  autoStart:false, maxValue: 100);
                model.MailTask = prog.AddTask("Mail", autoStart:false, maxValue: 100);
                model.WebTask  = prog.AddTask("Web",  autoStart:false, maxValue: 100);
                model.RepTask  = prog.AddTask("Reputation", autoStart:false, maxValue: 100);

                // Start all worker tasks
                var t1 = Task.Run(async () => { model.DnsTask!.StartTask(); await analyzers[0](); model.DnsTask!.Value = 100; });
                var t2 = Task.Run(async () => { model.MailTask!.StartTask(); await analyzers[1](); model.MailTask!.Value = 100; });
                var t3 = Task.Run(async () => { model.WebTask!.StartTask(); await analyzers[2](); model.WebTask!.Value = 100; });
                var t4 = Task.Run(async () => { model.RepTask!.StartTask(); await analyzers[3](); model.RepTask!.Value = 100; });

                await Task.WhenAll(t1,t2,t3,t4);
            });

        // After completion, swap progress with your final composed panels/trees
        ctx.UpdateTarget(new Columns(
            new Panel(new Markup("[green]All stages complete[/]")){Border=BoxBorder.Heavy}
        ));
    });
}
```

Pattern:

* Each analyzer runs in parallel, fills **DTOs** (e.g., `ScanResult`).
* UI shows **Progress** during run.
* When all done, replace the live surface with the final **Trees/Panels/Bars**.

---

## 6) Deterministic “Tree-first” final render

After background tasks finish, render **summary trees** and **posture panels** in a fixed order:

```csharp
public static void RenderFinal(ScanResult r)
{
    var col = new Columns();
    col.AddItem(new Panel(new Markup($"[bold]Domain:[/] {r.Domain}\n[grey]{r.StartedUtc:u} → {r.FinishedUtc:u}[/]"))
    { Header = new PanelHeader("[bold]Summary[/]") });

    AnsiConsole.Write(col);
    Ui.DnsTree(r.Dns, r.Domain);
    Ui.WebPanel(r.Web);
    UiBars.PostureBars(r.Mail.PolicyScore).WriteToConsole(); // or AnsiConsole.Write(...)
}
```

---

## 7) Where each widget shines (mapping)

* **Typewriter**: ephemeral logs, connection banners, “probes”, warnings.
* **Tree**: hierarchical records (DNS/MX/SPF/DKIM/DMARC), web features (HTTP→H2/H3/HSTS→headers), reputation (WHOIS→RDAP→RPKI).
* **Panels**: posture summaries and key-value overviews (DNSSEC, AXFR, Wildcard, Open Resolver, Min/Max TTL).
* **BarChart**: “scores” that would otherwise be hard to read as text (coverage/strength/completeness).
* **HeatMap/Canvas**: many-to-many status grids (propagation, header presence per path, cipher acceptance per MX).
* **Progress/Live**: parallel execution without interleaved output.

---

## 8) Minimal parallel orchestration (ready to drop in)

```csharp
// In your wizard:
var scan = new ScanResult { Domain = domain };
var dnsTask  = Task.Run(async () => { FxLog.Enqueue("[grey]> DNS…[/]"); await AnalyzeDnsAsync(scan); });
var mailTask = Task.Run(async () => { FxLog.Enqueue("[grey]> Mail…[/]"); await AnalyzeMailAsync(scan); });
var webTask  = Task.Run(async () => { FxLog.Enqueue("[grey]> Web…[/]");  await AnalyzeWebAsync(scan); });
var repTask  = Task.Run(async () => { FxLog.Enqueue("[grey]> Rep…[/]");  await AnalyzeRepAsync(scan); });

await RunParallelWithLiveAsync(
    new Func<Task>[] { () => dnsTask, () => mailTask, () => webTask, () => repTask },
    buildStaticSections: model => { /* optional: add static placeholders */ }
);

// Final render (deterministic, single thread)
RenderFinal(scan);
```

Analyzers should only:

* Fill `scan` DTOs (thread-safe if each stage writes to its own section).
* Optionally `FxLog.Enqueue(...)` info lines.
* Never directly call `AnsiConsole.Write(...)`.

---

## 9) Tree + Bars combo example (Mail stage final)

```csharp
public static void MailSummary(MailResult m)
{
    var tree = new Tree("[bold]Email[/]");
    var spf = tree.AddNode("[white]SPF[/]").AddNode((m.SpfRecord ?? "—").EscapeMarkup());
    var dmarc = tree.AddNode("[white]DMARC[/]").AddNode((m.DmarcRecord ?? "—").EscapeMarkup());

    var dkim = tree.AddNode("[white]DKIM[/]");
    foreach (var kv in m.DkimSelectorsOk)
        dkim.AddNode($"{kv.Key}: {(kv.Value==true?"[green]OK[/]": kv.Value==false?"[red]FAIL[/]":"[yellow]?[/]")}");

    var grid = new Grid().AddColumn().AddColumn();
    grid.AddRow(tree, UiBars.PostureBars(m.PolicyScore));

    AnsiConsole.Write(grid);
}
```

---

### TL;DR implementation strategy

1. **Run analyzers in parallel** → `Task.WhenAll`.
2. **Don’t print from background threads**. Push strings to a **channel**; consume on UI thread (typewriter).
3. Show **Progress/Live** while running; **replace** with final **Tree/Panel/Bar/HeatMap** after completion.
4. Use **Trees** for structure, **Panels** for summaries, **BarCharts** for posture scores, **HeatMap/Canvas** for grids.

If you want, I can fold these patterns directly into your `Wizard.cs` from the previous step—swapping the sequential per-stage printing with a **Live+Progress** parallel orchestrator and a consolidated final renderer.
