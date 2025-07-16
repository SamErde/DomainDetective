namespace DomainDetective.CLI;

using DomainDetective;
using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Settings for <see cref="TestRdapIpCommand"/>.
/// </summary>
internal sealed class TestRdapIpSettings : CommandSettings {
    /// <summary>IP address or range.</summary>
    [CommandArgument(0, "<ip>")]
    public string Ip { get; set; } = string.Empty;
}

/// <summary>
/// Queries RDAP information for an IP network.
/// </summary>
internal sealed class TestRdapIpCommand : AsyncCommand<TestRdapIpSettings> {
    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, TestRdapIpSettings settings) {
        var client = new RdapClient();
        var result = await client.GetIp(settings.Ip, Program.CancellationToken);
        if (result != null) {
            CliHelpers.ShowPropertiesTable($"RDAP IP {settings.Ip}", result, false);
        }
        return 0;
    }
}
