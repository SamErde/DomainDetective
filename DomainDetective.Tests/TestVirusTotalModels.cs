using System.Text.Json;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;

namespace DomainDetective.Tests;

public class TestVirusTotalModels
{
    [Fact]
    public void ResponseSerializationRoundTrip()
    {
        const string json = "{\"data\":{\"id\":\"1\",\"type\":\"domain\",\"attributes\":{\"last_analysis_stats\":{\"malicious\":1},\"reputation\":42}}}";
        var resp = JsonSerializer.Deserialize<VirusTotalResponse>(json, VirusTotalJson.Options)!;
        Assert.Equal("1", resp.Data?.Id);
        Assert.Equal(VirusTotalObjectType.Domain, resp.Data?.Type);
        Assert.Equal(1, resp.Data?.Attributes?.LastAnalysisStats?.Malicious);
        Assert.Equal(42, resp.Data?.Attributes?.Reputation);

        var round = JsonSerializer.Deserialize<VirusTotalResponse>(JsonSerializer.Serialize(resp, VirusTotalJson.Options), VirusTotalJson.Options)!;
        Assert.Equal(resp.Data?.Type, round.Data?.Type);
    }

    [Fact]
    public async Task ClientQueriesEndpoint()
    {
        var handler = new MockHttpMessageHandler();
        handler.When("https://api/v3/domains/*").Respond("application/json", "{\"data\":{\"id\":\"x\",\"type\":\"domain\",\"attributes\":{\"last_analysis_stats\":{\"malicious\":0}}}}");
        var client = new VirusTotalClient(baseUrl: "https://api/v3") { HttpHandlerFactory = () => handler };
        var result = await client.GetDomain("example.com");
        Assert.Equal("x", result?.Data?.Id);
    }
}
