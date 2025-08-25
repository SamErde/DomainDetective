using Spectre.Console;

namespace DomainDetective.CLI;

internal static class MarkupExtensions
{
    public static string EscapeMarkup(this string? value)
        => Markup.Escape(value ?? string.Empty);
}

