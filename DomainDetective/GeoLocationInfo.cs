namespace DomainDetective;

/// <summary>
/// Geolocation information for an IP address.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public sealed class GeoLocationInfo {
    /// <summary>Country where the IP is located.</summary>
    public string? Country { get; init; }

    /// <summary>Region where the IP is located.</summary>
    public string? Region { get; init; }
}
