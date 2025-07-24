namespace DomainDetective;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides JSON serializer options for Autodiscover endpoint results.
/// </summary>
public static class AutodiscoverJson {
    /// <summary>Default serializer options.</summary>
    public static readonly JsonSerializerOptions Options;

    static AutodiscoverJson() {
        Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Options.Converters.Add(new JsonStringEnumConverter());
    }
}