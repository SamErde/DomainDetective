using Spectre.Console;

namespace DomainDetective.CLI;

internal static class ProgressContextExtensions
{
    public static void Dispose(this ProgressContext context)
    {
        // No-op for Spectre.Console versions lacking IDisposable
    }
}
