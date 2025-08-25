using DnsClientX;
using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace DomainDetective.PowerShell {
    /// <summary>Validates DMARC record for a domain.</summary>
    /// <para>Part of the DomainDetective project.</para>
    /// <example>
    ///   <summary>Check DMARC settings.</summary>
    ///   <code>Test-EmailDmarc -DomainName example.com</code>
    /// </example>
[Cmdlet(VerbsDiagnostic.Test, "DDEmailDmarcRecord", DefaultParameterSetName = "ServerName")]
[Alias("Test-EmailDmarc")]
    public sealed class CmdletTestDmarcRecord : AsyncPSCmdlet {
        /// <summary>Domain to query.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ServerName")]
        [ValidateNotNullOrEmpty]
        public string DomainName;

        /// <summary>DNS server used for queries.</summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ServerName")]
        public DnsEndpoint DnsEndpoint = DnsEndpoint.System;

        /// <summary>Return raw analysis object.</summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Raw;

        private InternalLogger _logger;
        private DomainHealthCheck healthCheck;

        /// <summary>Initializes logging and helper classes.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        protected override Task BeginProcessingAsync() {
            _logger = new InternalLogger(false);
            var internalLoggerPowerShell = new InternalLoggerPowerShell(_logger, this.WriteVerbose, this.WriteWarning, this.WriteDebug, this.WriteError, this.WriteProgress, this.WriteInformation);
            internalLoggerPowerShell.ResetActivityIdCounter();
            healthCheck = new DomainHealthCheck(DnsEndpoint, _logger);
            return Task.CompletedTask;
        }

        /// <summary>Executes the cmdlet operation.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        protected override async Task ProcessRecordAsync() {
            _logger.WriteVerbose("Querying DMARC record for domain: {0}", DomainName);
            await healthCheck.VerifyDMARC(DomainName);
            if (!string.IsNullOrEmpty(healthCheck.DmarcAnalysis.Advisory)) {
                WriteInformation(healthCheck.DmarcAnalysis.Advisory, Array.Empty<string>());
            }
            if (Raw) {
                WriteObject(healthCheck.DmarcAnalysis);
            } else {
                var output = OutputHelper.Convert(healthCheck.DmarcAnalysis);
                WriteObject(output);
            }
        }
    }
}