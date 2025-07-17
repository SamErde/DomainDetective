namespace DomainDetective;

using System.Text.Json;

/// <summary>
/// Provides JSON serializer options for VirusTotal objects.
/// </summary>
public static class VirusTotalJson
{
    /// <summary>Default serializer options.</summary>
    public static readonly JsonSerializerOptions Options;

    static VirusTotalJson()
    {
        Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Options.Converters.Add(new VirusTotalObjectTypeConverter());
    }
}
