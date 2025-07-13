using DomainDetective.CLI;

namespace DomainDetective.CLI.Tests;

public class TestCheckDomainSettings
{
    [Fact]
    public void DefaultTakeoverOptionFalse()
    {
        var settings = new CheckDomainSettings();
        Assert.False(settings.CheckTakeover);
    }
}
