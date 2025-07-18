using DnsClientX;
using System.Management.Automation;
using System.Threading.Tasks;
using DomainDetective;

namespace DomainDetective.PowerShell {
    /// <summary>Runs multiple domain health checks and returns the results.</summary>
    /// <para>Part of the DomainDetective project.</para>
    /// <example>
    ///   <summary>Perform a full health test.</summary>
    ///   <code>Test-DomainHealth -DomainName example.com -Verbose</code>
    /// </example>
[Cmdlet(VerbsDiagnostic.Test, "DDDomainOverallHealth", DefaultParameterSetName = "ServerName")]
[Alias("Test-DomainHealth")]
    [OutputType(typeof(DomainHealthCheck))]
    public sealed class CmdletTestDomainHealth : AsyncPSCmdlet {
        /// <summary>Domain to analyze.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ServerName")]
        public string DomainName;

        /// <summary>DNS server used for queries.</summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ServerName")]
        public DnsEndpoint DnsEndpoint = DnsEndpoint.System;

        /// <summary>Specific tests to run.</summary>
        [Parameter(Mandatory = false)]
        public HealthCheckType[]? HealthCheckType;

        /// <summary>DKIM selectors used when testing DKIM.</summary>
        [Parameter(Mandatory = false)]
        public string[]? DkimSelectors;

        /// <summary>Service types to check for DANE. HTTPS (port 443) is queried by default.</summary>
        [Parameter(Mandatory = false)]
        public ServiceType[]? DaneServiceType;

        /// <summary>Custom ports to check for DANE.</summary>
        [Parameter(Mandatory = false)]
        public int[]? DanePorts;

        /// <summary>Protected brand terms for typosquatting analysis.</summary>
        [Parameter(Mandatory = false)]
        public string[]? BrandKeyword;
        
        /// <summary>Port scan profiles to use.</summary>
        [Parameter(Mandatory = false)]
        public PortScanProfile[]? PortScanProfile;

        private InternalLogger _logger;
        private DomainHealthCheck _healthCheck;

        /// <summary>Initializes logging and helper classes.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        protected override Task BeginProcessingAsync() {
            _logger = new InternalLogger(false);
            var internalLoggerPowerShell = new InternalLoggerPowerShell(
                _logger,
                this.WriteVerbose,
                this.WriteWarning,
                this.WriteDebug,
                this.WriteError,
                this.WriteProgress,
                this.WriteInformation);
            internalLoggerPowerShell.ResetActivityIdCounter();
            _healthCheck = new DomainHealthCheck(DnsEndpoint, _logger);
            if (BrandKeyword != null)
            {
                _healthCheck.TyposquattingBrandKeywords.AddRange(BrandKeyword);
            }
            return Task.CompletedTask;
        }

        /// <summary>Executes the cmdlet operation.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        protected override async Task ProcessRecordAsync() {
            _logger.WriteVerbose("Querying domain health for domain: {0}", DomainName);
            if (BrandKeyword != null) {
                _healthCheck.TyposquattingBrandKeywords.Clear();
                _healthCheck.TyposquattingBrandKeywords.AddRange(BrandKeyword);
            }
            await _healthCheck.Verify(DomainName, HealthCheckType, DkimSelectors, DaneServiceType, DanePorts, PortScanProfile);
            var result = _healthCheck.FilterAnalyses(HealthCheckType);
            WriteObject(result);
        }
    }
}