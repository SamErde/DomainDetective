using System.Text.Json;

namespace DomainDetective.Tests;

public class TestJsonOptions
{
    [Fact]
    public void DefaultMatchesHealthCheck()
    {
        Assert.Same(JsonOptions.Default, DomainHealthCheck.JsonOptions);
        Assert.True(JsonOptions.Default.WriteIndented);
        Assert.Contains(JsonOptions.Default.Converters, c => c is IPAddressJsonConverter);
    }
}
