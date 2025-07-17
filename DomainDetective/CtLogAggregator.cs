using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective;

/// <summary>
/// Retrieves certificate transparency log entries from multiple APIs.
/// </summary>
public sealed class CtLogAggregator
{
    /// <summary>CT log API templates containing a {0} placeholder for the fingerprint.</summary>
    public List<string> ApiTemplates { get; } = new() { "https://crt.sh/?sha256={0}&output=json" };

    /// <summary>Optional HTTP handler factory for testing.</summary>
    internal Func<HttpMessageHandler>? HttpHandlerFactory { get; set; }

    /// <summary>Optional override returning the JSON content for a fingerprint.</summary>
    public Func<string, Task<string>>? QueryOverride { get; set; }

    /// <summary>
    /// Queries all configured APIs for the specified SHA-256 fingerprint.
    /// </summary>
    /// <param name="fingerprint">SHA-256 certificate fingerprint.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>List of JSON elements describing log entries.</returns>
    public async Task<IReadOnlyList<JsonElement>> QueryAsync(string fingerprint, CancellationToken cancellationToken = default)
    {
        var results = new List<JsonElement>();
        if (string.IsNullOrWhiteSpace(fingerprint))
        {
            return results;
        }

        if (QueryOverride != null)
        {
            var json = await QueryOverride(fingerprint).ConfigureAwait(false);
            AppendEntries(json, results);
            return results;
        }

        var client = CreateClient(out var dispose);
        try
        {
            foreach (var template in ApiTemplates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var url = string.Format(template, fingerprint);
                using var resp = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
                    continue;
                }
                var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                AppendEntries(json, results);
            }
        }
        finally
        {
            if (dispose)
            {
                client.Dispose();
            }
        }

        return results;
    }

    private static void AppendEntries(string json, ICollection<JsonElement> results)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in doc.RootElement.EnumerateArray())
                {
                    results.Add(entry.Clone());
                }
            }
            else if (doc.RootElement.ValueKind != JsonValueKind.Undefined && doc.RootElement.ValueKind != JsonValueKind.Null)
            {
                results.Add(doc.RootElement.Clone());
            }
        }
        catch
        {
            // ignore parse errors
        }
    }

    private HttpClient CreateClient(out bool dispose)
    {
        if (HttpHandlerFactory != null)
        {
            dispose = true;
            return new HttpClient(HttpHandlerFactory(), disposeHandler: true);
        }

        dispose = false;
        return SharedHttpClient.Instance;
    }
}
