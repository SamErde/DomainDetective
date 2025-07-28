namespace DomainDetective.Tests;

public static class Skip
{
    public static void If(bool condition, string? reason = null)
    {
        if (condition)
        {
            throw Xunit.Sdk.SkipException.ForSkip(reason);
        }
    }

    public static void IfNot(bool condition, string? reason = null)
    {
        if (!condition)
        {
            throw Xunit.Sdk.SkipException.ForSkip(reason);
        }
    }
}
