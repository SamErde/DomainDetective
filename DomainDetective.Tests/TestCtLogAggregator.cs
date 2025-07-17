using System.Text.Json;
using System.Threading.Tasks;
using DomainDetective;
using RichardSzalay.MockHttp;

namespace DomainDetective.Tests;

public class TestCtLogAggregator
{
    [Fact]
    public async Task AggregatesEntriesFromApis()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("https://api1/*").Respond("application/json", "[{\"id\":1}]");
        mock.When("https://api2/*").Respond("application/json", "[{\"id\":2}]");

        var aggregator = new CtLogAggregator { HttpHandlerFactory = () => mock };
        aggregator.ApiTemplates.Clear();
        aggregator.ApiTemplates.Add("https://api1/{0}");
        aggregator.ApiTemplates.Add("https://api2/{0}");

        var entries = await aggregator.QueryAsync("abc");

        Assert.Equal(2, entries.Count);
        Assert.Equal(1, entries[0].GetProperty("id").GetInt32());
        Assert.Equal(2, entries[1].GetProperty("id").GetInt32());
    }
}
