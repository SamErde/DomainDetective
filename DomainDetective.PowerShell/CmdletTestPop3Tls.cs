using System.Management.Automation;
using System.Threading.Tasks;

namespace DomainDetective.PowerShell {
    /// <summary>Checks TLS configuration for a specific POP3 host.</summary>
    /// <para>Part of the DomainDetective project.</para>
    /// <example>
    ///   <summary>Test POP3 TLS.</summary>
    ///   <code>Test-DDEmailPop3Tls -HostName mail.example.com -Port 995</code>
    /// </example>
    [Cmdlet(VerbsDiagnostic.Test, "DDEmailPop3Tls", DefaultParameterSetName = "ServerName")]
    [Alias("Test-EmailPop3Tls", "Test-Pop3Tls")]
    public sealed class CmdletTestPop3Tls : AsyncPSCmdlet {
        /// <summary>POP3 host to check.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ServerName")]
        public string HostName;

        /// <summary>POP3 port number.</summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ServerName")]
        public int Port = 110;

        /// <summary>Output certificate chain information.</summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter ShowChain;

        private InternalLogger _logger;
        private DomainHealthCheck _healthCheck;

        /// <summary>Initializes logging and helper classes.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        protected override Task BeginProcessingAsync() {
            _logger = new InternalLogger(false);
            var internalLoggerPowerShell = new InternalLoggerPowerShell(_logger, this.WriteVerbose, this.WriteWarning, this.WriteDebug, this.WriteError, this.WriteProgress, this.WriteInformation);
            internalLoggerPowerShell.ResetActivityIdCounter();
            _healthCheck = new DomainHealthCheck(internalLogger: _logger);
            return Task.CompletedTask;
        }

        /// <summary>Executes the cmdlet operation.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        protected override async Task ProcessRecordAsync() {
            _logger.WriteVerbose("Checking POP3 TLS for {0}:{1}", HostName, Port);
            await _healthCheck.CheckPop3TlsHost(HostName, Port);
            var result = _healthCheck.Pop3TlsAnalysis.ServerResults[$"{HostName}:{Port}"];
            WriteObject(result);
            if (ShowChain && result.Chain.Count > 0) {
                WriteObject(result.Chain, true);
            }
        }
    }
}
