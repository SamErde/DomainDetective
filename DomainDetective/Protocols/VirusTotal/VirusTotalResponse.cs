namespace DomainDetective;

using System.Text.Json.Serialization;

/// <summary>
/// Response wrapper returned by VirusTotal API.
/// </summary>
public sealed class VirusTotalResponse
{
    /// <summary>Data portion of the response.</summary>
    [JsonPropertyName("data")]
    public VirusTotalObject? Data { get; set; }
}
