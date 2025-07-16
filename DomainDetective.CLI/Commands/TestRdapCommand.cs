using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective.CLI;

/// <summary>
/// Settings for <see cref="TestRdapCommand"/>.
/// </summary>
internal sealed class TestRdapSettings : CommandSettings {
    /// <summary>Domain to query.</summary>
    [CommandArgument(0, "<domain>")]
    public string Domain { get; set; } = string.Empty;
}

/// <summary>
/// Queries RDAP registration information.
/// </summary>
internal sealed class TestRdapCommand : AsyncCommand<TestRdapSettings> {
    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, TestRdapSettings settings) {
        var hc = new DomainHealthCheck();
        await hc.QueryRDAP(settings.Domain, Program.CancellationToken);
        CliHelpers.ShowPropertiesTable($"RDAP for {settings.Domain}", hc.RdapAnalysis, false);
        return 0;
    }
}
