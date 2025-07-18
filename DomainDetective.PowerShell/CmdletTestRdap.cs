using DnsClientX;
using System.Management.Automation;
using System.Threading.Tasks;

namespace DomainDetective.PowerShell {
    /// <summary>Queries RDAP registration information.</summary>
    /// <para>Part of the DomainDetective project.</para>
    /// <example>
    ///   <summary>Query RDAP.</summary>
    ///   <code>Test-Rdap -DomainName example.com</code>
    /// </example>
    [Cmdlet(VerbsDiagnostic.Test, "DDRdap", DefaultParameterSetName = "ServerName")]
    [Alias("Test-Rdap")]
    public sealed class CmdletTestRdap : AsyncPSCmdlet {
        /// <summary>Domain to query.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ServerName")]
        [ValidateNotNullOrEmpty]
        public string DomainName;

        /// <summary>DNS server used for queries.</summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ServerName")]
        public DnsEndpoint DnsEndpoint = DnsEndpoint.System;

        private InternalLogger _logger;
        private DomainHealthCheck _healthCheck;

        /// <summary>Initializes logging and helper classes.</summary>
        protected override Task BeginProcessingAsync() {
            _logger = new InternalLogger(false);
            var psLogger = new InternalLoggerPowerShell(_logger, WriteVerbose, WriteWarning, WriteDebug, WriteError, WriteProgress, WriteInformation);
            psLogger.ResetActivityIdCounter();
            _healthCheck = new DomainHealthCheck(DnsEndpoint, _logger);
            return Task.CompletedTask;
        }

        /// <summary>Executes the cmdlet operation.</summary>
        protected override async Task ProcessRecordAsync() {
            _logger.WriteVerbose("Querying RDAP for domain: {0}", DomainName);
            await _healthCheck.QueryRDAP(DomainName);
            WriteObject(_healthCheck.RdapAnalysis);
        }
    }
}
