namespace DomainDetective
{
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides high-level methods for querying search engines.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public sealed class SearchEngineAnalysis
{
    /// <summary>Google search client.</summary>
    public GoogleSearchClient Google { get; } = new();

    /// <summary>Bing search client.</summary>
    public BingSearchClient Bing { get; } = new();

    /// <summary>Endpoint for Google Custom Search API.</summary>
    public string GoogleEndpoint
    {
        get => Google.BaseUrl;
        set => Google.BaseUrl = value;
    }

    /// <summary>Google API key.</summary>
    public string? GoogleApiKey
    {
        get => Google.ApiKey;
        set => Google.ApiKey = value;
    }

    /// <summary>Google search engine identifier.</summary>
    public string? GoogleCx
    {
        get => Google.EngineId;
        set => Google.EngineId = value;
    }

    /// <summary>Endpoint for Bing Web Search API.</summary>
    public string BingEndpoint
    {
        get => Bing.BaseUrl;
        set => Bing.BaseUrl = value;
    }

    /// <summary>Bing API key.</summary>
    public string? BingApiKey
    {
        get => Bing.ApiKey;
        set => Bing.ApiKey = value;
    }

    /// <summary>Factory for custom HTTP handlers used in tests.</summary>
    internal Func<HttpMessageHandler>? HttpHandlerFactory
    {
        get => Google.HttpHandlerFactory;
        set
        {
            Google.HttpHandlerFactory = value;
            Bing.HttpHandlerFactory = value;
        }
    }

    /// <summary>Executes a Google Custom Search query.</summary>
    public Task<GoogleSearchResponse> SearchGoogle(string query, CancellationToken ct = default)
        => Google.QueryAsync(query, ct);

    /// <summary>Executes a Bing Web Search query.</summary>
    public Task<BingSearchResponse> SearchBing(string query, CancellationToken ct = default)
        => Bing.QueryAsync(query, ct);
}

}
