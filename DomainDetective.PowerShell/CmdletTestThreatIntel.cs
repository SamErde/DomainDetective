using System.Management.Automation;
using System.Threading.Tasks;

namespace DomainDetective.PowerShell;

/// <summary>Queries reputation services for a domain or IP address.</summary>
/// <para>Part of the DomainDetective project.</para>
/// <example>
///   <summary>Check reputation listings.</summary>
///   <code>Test-DomainThreatIntel -NameOrIpAddress example.com</code>
/// </example>
[Cmdlet(VerbsDiagnostic.Test, "DDThreatIntel")]
[Alias("Test-DomainThreatIntel")]
public sealed class CmdletTestThreatIntel : AsyncPSCmdlet {
    /// <summary>Domain or IP address to query.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty]
    public string NameOrIpAddress;

    /// <summary>Google Safe Browsing API key.</summary>
    [Parameter(Mandatory = false)]
    public string? GoogleApiKey;

    /// <summary>PhishTank API key.</summary>
    [Parameter(Mandatory = false)]
    public string? PhishTankApiKey;

    /// <summary>VirusTotal API key.</summary>
    [Parameter(Mandatory = false)]
    public string? VirusTotalApiKey;

    private InternalLogger _logger;
    private DomainHealthCheck _healthCheck;

        /// <summary>Initializes logging and helper classes.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
    protected override Task BeginProcessingAsync() {
        _logger = new InternalLogger(false);
        var loggerPs = new InternalLoggerPowerShell(
            _logger,
            this.WriteVerbose,
            this.WriteWarning,
            this.WriteDebug,
            this.WriteError,
            this.WriteProgress,
            this.WriteInformation);
        loggerPs.ResetActivityIdCounter();
        _healthCheck = new DomainHealthCheck(internalLogger: _logger);
        return Task.CompletedTask;
    }

        /// <summary>Executes the cmdlet operation.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
    protected override async Task ProcessRecordAsync() {
        _healthCheck.GoogleSafeBrowsingApiKey = GoogleApiKey;
        _healthCheck.PhishTankApiKey = PhishTankApiKey;
        _healthCheck.VirusTotalApiKey = VirusTotalApiKey;

        _logger.WriteVerbose("Querying threat intel for {0}", NameOrIpAddress);
        await _healthCheck.VerifyThreatIntel(NameOrIpAddress);
        WriteObject(_healthCheck.ThreatIntelAnalysis);
    }
}
