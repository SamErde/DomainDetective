using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DomainDetective.Tests;

public class TestSearchEngineAnalysis
{
    [Fact]
    public async Task SearchGoogleReturnsJson()
    {
        var handler = new MockHttpMessageHandler();
        handler.When("https://search.test/*").Respond(
            "application/json",
            "{\"items\":[{\"title\":\"t\",\"cacheId\":\"c1\",\"htmlSnippet\":\"<b>x\"}],\"searchInformation\":{\"totalResults\":\"1\",\"searchTime\":0.01}}");
        var analysis = new SearchEngineAnalysis
        {
            GoogleApiKey = "key",
            GoogleCx = "cx",
            GoogleEndpoint = "https://search.test"
        };
        analysis.HttpHandlerFactory = () => handler;

        var result = await analysis.SearchGoogle("test");
        Assert.Single(result.Items);
        Assert.Equal("1", result.Info?.TotalResults);
        Assert.Equal("c1", result.Items[0].CacheId);
    }

    [Fact]
    public async Task SearchBingHandlesErrors()
    {
        var handler = new MockHttpMessageHandler();
        handler.When("https://bing.test/*").Respond(HttpStatusCode.BadRequest);
        var analysis = new SearchEngineAnalysis
        {
            BingApiKey = "key",
            BingEndpoint = "https://bing.test"
        };
        analysis.HttpHandlerFactory = () => handler;

        await Assert.ThrowsAsync<HttpRequestException>(() => analysis.SearchBing("x"));
    }
}
