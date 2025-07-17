using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace DomainDetective.CLI;

/// <summary>
/// Settings for <see cref="SuggestDomainCommand"/>.
/// </summary>
internal sealed class SuggestDomainSettings : CommandSettings
{
    /// <summary>Domain to check.</summary>
    [CommandArgument(0, "<domain>")]
    public string Domain { get; set; } = string.Empty;
}

/// <summary>
/// Suggests available domains using alternative TLDs.
/// </summary>
internal sealed class SuggestDomainCommand : AsyncCommand<SuggestDomainSettings>
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, SuggestDomainSettings settings)
    {
        var search = new DomainAvailabilitySearch();
        await foreach (var result in search.CheckTldAlternativesAsync(settings.Domain, Program.CancellationToken))
        {
            if (result.Available)
            {
                AnsiConsole.MarkupLine($"[green]{result.Domain}[/]");
            }
        }

        return 0;
    }
}
