namespace DomainDetective;

using System.Text.Json.Serialization;

/// <summary>
/// Statistics returned by VirusTotal analysis.
/// </summary>
public sealed class VirusTotalStats
{
    /// <summary>Count of harmless detections.</summary>
    [JsonPropertyName("harmless")]
    public int Harmless { get; set; }

    /// <summary>Count of malicious detections.</summary>
    [JsonPropertyName("malicious")]
    public int Malicious { get; set; }

    /// <summary>Count of suspicious detections.</summary>
    [JsonPropertyName("suspicious")]
    public int Suspicious { get; set; }

    /// <summary>Count of undetected results.</summary>
    [JsonPropertyName("undetected")]
    public int Undetected { get; set; }

    /// <summary>Count of timeout results.</summary>
    [JsonPropertyName("timeout")]
    public int Timeout { get; set; }

    /// <summary>Count of unsupported type results.</summary>
    [JsonPropertyName("type-unsupported")]
    public int TypeUnsupported { get; set; }
}
