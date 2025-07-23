namespace DomainDetective
{
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Client for Bing Web Search API.
/// </summary>
public sealed class BingSearchClient
{
    /// <summary>API endpoint.</summary>
    public string BaseUrl { get; set; } = "https://api.bing.microsoft.com/v7.0/search";

    /// <summary>API key.</summary>
    public string? ApiKey { get; set; }

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
    public async Task<BingSearchResponse> QueryAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new InvalidOperationException("ApiKey must be set.");
        }

        var client = GetClient(out var dispose);
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}?q={Uri.EscapeDataString(query)}");
            request.Headers.Add("Ocp-Apim-Subscription-Key", ApiKey);
            using var resp = await client.SendAsync(request, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<BingSearchResponse>(json, SearchEngineJson.Options)!;
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
