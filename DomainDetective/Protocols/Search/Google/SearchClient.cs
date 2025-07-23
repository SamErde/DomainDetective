namespace DomainDetective
{
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Client for Google Custom Search API.
/// </summary>
public sealed class GoogleSearchClient
{
    /// <summary>API endpoint.</summary>
    public string BaseUrl { get; set; } = "https://www.googleapis.com/customsearch/v1";

    /// <summary>API key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Search engine identifier.</summary>
    public string? EngineId { get; set; }

    /// <summary>Factory for custom HTTP handlers.</summary>
    internal Func<HttpMessageHandler>? HttpHandlerFactory { get; set; }

    private HttpClient GetClient(out bool dispose)
    {
        if (HttpHandlerFactory != null)
        {
            dispose = true;
            return new HttpClient(HttpHandlerFactory(), disposeHandler: true);
        }

        dispose = false;
        return SharedHttpClient.Instance;
    }

    /// <summary>Executes a search query.</summary>
    public async Task<GoogleSearchResponse> QueryAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (string.IsNullOrWhiteSpace(ApiKey) || string.IsNullOrWhiteSpace(EngineId))
        {
            throw new InvalidOperationException("ApiKey and EngineId must be set.");
        }

        var client = GetClient(out var dispose);
        try
        {
            var url = $"{BaseUrl}?key={ApiKey}&cx={EngineId}&q={Uri.EscapeDataString(query)}";
            using var resp = await client.GetAsync(url, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<GoogleSearchResponse>(json, SearchEngineJson.Options)!;
        }
        finally
        {
            if (dispose)
            {
                client.Dispose();
            }
        }
    }
}
}
