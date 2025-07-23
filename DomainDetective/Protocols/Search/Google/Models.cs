namespace DomainDetective
{
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Item returned by Google Custom Search.
/// </summary>
public sealed class GoogleSearchItem
{
    /// <summary>Result title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>Result URL.</summary>
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    /// <summary>Result snippet.</summary>
    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    /// <summary>Display URL.</summary>
    [JsonPropertyName("displayLink")]
    public string? DisplayLink { get; set; }

    /// <summary>Formatted URL.</summary>
    [JsonPropertyName("formattedUrl")]
    public string? FormattedUrl { get; set; }

    /// <summary>Cache identifier.</summary>
    [JsonPropertyName("cacheId")]
    public string? CacheId { get; set; }

    /// <summary>Title with formatting.</summary>
    [JsonPropertyName("htmlTitle")]
    public string? HtmlTitle { get; set; }

    /// <summary>Snippet with formatting.</summary>
    [JsonPropertyName("htmlSnippet")]
    public string? HtmlSnippet { get; set; }
}

/// <summary>
/// Google search response wrapper.
/// </summary>
public sealed class GoogleSearchResponse
{
    /// <summary>Returned items.</summary>
    [JsonPropertyName("items")]
    public List<GoogleSearchItem> Items { get; set; } = new();

    /// <summary>Search metadata.</summary>
    [JsonPropertyName("searchInformation")]
    public GoogleSearchInfo? Info { get; set; }
}

/// <summary>Additional information about the search.</summary>
public sealed class GoogleSearchInfo
{
    /// <summary>Number of results.</summary>
    [JsonPropertyName("totalResults")]
    public string? TotalResults { get; set; }

    /// <summary>Search time in seconds.</summary>
    [JsonPropertyName("searchTime")]
    public double SearchTime { get; set; }
}

}
