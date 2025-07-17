namespace DomainDetective;

using System.Text.Json.Serialization;

/// <summary>
/// VirusTotal object types.
/// </summary>
[JsonConverter(typeof(VirusTotalObjectTypeConverter))]
public enum VirusTotalObjectType
{
    /// <summary>Unknown or unsupported type.</summary>
    Unknown,
    /// <summary>IP address object.</summary>
    IpAddress,
    /// <summary>Domain object.</summary>
    Domain,
    /// <summary>URL object.</summary>
    Url
}
