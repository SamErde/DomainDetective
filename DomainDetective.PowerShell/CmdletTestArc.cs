using DnsClientX;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

namespace DomainDetective.PowerShell {
    /// <summary>Validates ARC headers from raw input.</summary>
    /// <para>Part of the DomainDetective project.</para>
    /// <example>
    ///   <summary>Analyze ARC headers from a file.</summary>
    ///   <code>Test-EmailArc -File './headers.txt'</code>
    /// </example>
    /// <example>
    ///   <summary>Analyze ARC headers from pipeline input.</summary>
    ///   <code>Get-Content './headers.txt' -Raw | Test-EmailArc</code>
    /// </example>
[Cmdlet(VerbsDiagnostic.Test, "DDEmailArcRecord", DefaultParameterSetName = "Text")]
[Alias("Test-EmailArc")]
    public sealed class CmdletTestArc : AsyncPSCmdlet {
        /// <para>Raw header text.</para>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Text", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string HeaderText { get; set; } = string.Empty;

        /// <para>Path to a file containing ARC headers.</para>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "File")]
        [ValidateNotNullOrEmpty]
        public string File { get; set; } = string.Empty;

        private InternalLogger _logger;
        private DomainHealthCheck _healthCheck;

        /// <summary>
        /// Initializes logging and the ARC health checker.
        /// </summary>
        /// <returns>A completed task.</returns>
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
            _healthCheck = new DomainHealthCheck(internalLogger: _logger);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates the ARC headers and writes the result to the pipeline.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task ProcessRecordAsync() {
            var text = ParameterSetName == "File"
                ? System.IO.File.ReadAllText(File)
                : HeaderText;
            var result = await _healthCheck.VerifyARCAsync(text, CancelToken);
            WriteObject(result);
        }
    }
}
