namespace DomainDetective
{
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Web page result returned by Bing search API.
/// </summary>
public sealed class BingSearchWebPage
{
    /// <summary>Result title.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Result URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Result snippet.</summary>
    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    /// <summary>Date last crawled.</summary>
    [JsonPropertyName("dateLastCrawled")]
    public string? DateLastCrawled { get; set; }

    /// <summary>Language code of the result.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>Indicates whether the result is family friendly.</summary>
    [JsonPropertyName("isFamilyFriendly")]
    public bool? IsFamilyFriendly { get; set; }
}

/// <summary>
/// Container for web page results.
/// </summary>
public sealed class BingSearchWebPages
{
    /// <summary>Result list.</summary>
    [JsonPropertyName("value")]
    public List<BingSearchWebPage> Value { get; set; } = new();

    /// <summary>Total estimated matches.</summary>
    [JsonPropertyName("totalEstimatedMatches")]
    public long TotalEstimatedMatches { get; set; }
}

/// <summary>
/// Bing search response wrapper.
/// </summary>
public sealed class BingSearchResponse
{
    /// <summary>Information about the executed query.</summary>
    [JsonPropertyName("queryContext")]
    public BingQueryContext? QueryContext { get; set; }

    /// <summary>Web pages results container.</summary>
    [JsonPropertyName("webPages")]
    public BingSearchWebPages? WebPages { get; set; }
}

/// <summary>Query context data.</summary>
public sealed class BingQueryContext
{
    /// <summary>Original query string.</summary>
    [JsonPropertyName("originalQuery")]
    public string? OriginalQuery { get; set; }
}

}
