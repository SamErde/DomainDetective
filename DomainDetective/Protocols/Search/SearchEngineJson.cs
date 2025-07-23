namespace DomainDetective
{
using System.Text.Json;

/// <summary>
/// Provides JSON serializer options for search engine responses.
/// </summary>
public static class SearchEngineJson
{
    /// <summary>Default serializer options.</summary>
    public static readonly JsonSerializerOptions Options;

    static SearchEngineJson()
    {
        Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }
}
}
