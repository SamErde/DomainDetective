using DnsClientX;
using System.Management.Automation;
using System.Threading.Tasks;

namespace DomainDetective.PowerShell {
    /// <summary>Validates SMIMEA records for the given email address.</summary>
    /// <para>Part of the DomainDetective project.</para>
    /// <example>
    ///   <summary>Check SMIMEA record.</summary>
    ///   <code>Test-DnsSmimea -EmailAddress user@example.com</code>
    /// </example>
[Cmdlet(VerbsDiagnostic.Test, "DDDnsSmimeaRecord", DefaultParameterSetName = "Email")]
[Alias("Test-DnsSmimea")]
    public sealed class CmdletTestSmimeaRecord : AsyncPSCmdlet {
        /// <summary>Email address to query.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Email")]
        [ValidateNotNullOrEmpty]
        public string EmailAddress;

        /// <summary>DNS server used for queries.</summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "Email")]
        public DnsEndpoint DnsEndpoint = DnsEndpoint.System;

        private InternalLogger _logger;
        private DomainHealthCheck _healthCheck;

        /// <summary>Initializes logging and helper classes.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        protected override Task BeginProcessingAsync() {
            _logger = new InternalLogger(false);
            var psLogger = new InternalLoggerPowerShell(_logger, WriteVerbose, WriteWarning, WriteDebug, WriteError, WriteProgress, WriteInformation);
            psLogger.ResetActivityIdCounter();
            _healthCheck = new DomainHealthCheck(DnsEndpoint, _logger);
            return Task.CompletedTask;
        }

        /// <summary>Executes the cmdlet operation.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        protected override async Task ProcessRecordAsync() {
            _logger.WriteVerbose("Querying SMIMEA record for {0}", EmailAddress);
            await _healthCheck.VerifySMIMEA(EmailAddress);
            WriteObject(_healthCheck.SmimeaAnalysis);
        }
    }
}
