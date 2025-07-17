using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective.CLI;

/// <summary>
/// Settings for <see cref="TestNtpServerCommand"/>.
/// </summary>
internal sealed class TestNtpServerSettings : CommandSettings {
    /// <summary>NTP server to query.</summary>
    [CommandArgument(0, "<server>")]
    public string Server { get; set; } = string.Empty;

    /// <summary>NTP port number.</summary>
    [CommandOption("--port")]
    public int Port { get; set; } = 123;
}

/// <summary>
/// Queries an NTP server for clock offset.
/// </summary>
internal sealed class TestNtpServerCommand : AsyncCommand<TestNtpServerSettings> {
    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, TestNtpServerSettings settings) {
        var hc = new DomainHealthCheck();
        await hc.TestNtpServer(settings.Server, settings.Port, Program.CancellationToken);
        CliHelpers.ShowPropertiesTable($"NTP {settings.Server}", hc.NtpAnalysis.ServerResults, false);
        return 0;
    }
}
