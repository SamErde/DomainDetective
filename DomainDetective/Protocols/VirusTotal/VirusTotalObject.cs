namespace DomainDetective;

using System.Text.Json.Serialization;

/// <summary>
/// VirusTotal object container.
/// </summary>
public sealed class VirusTotalObject
{
    /// <summary>Identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Object type.</summary>
    [JsonPropertyName("type")]
    public VirusTotalObjectType Type { get; set; }

    /// <summary>Object attributes.</summary>
    [JsonPropertyName("attributes")]
    public VirusTotalAttributes? Attributes { get; set; }
}
