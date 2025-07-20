namespace DomainDetective;

using System.ComponentModel;

/// <summary>
/// TLSA selector values defined in RFC 6698.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public enum TlsaSelector {
    /// <summary>Selector value not recognized.</summary>
    [Description("Unknown")] Unknown = -1,
    /// <summary>Cert: Full Certificate.</summary>
    [Description("Cert: Full Certificate")] Cert = 0,
    /// <summary>SPKI: SubjectPublicKeyInfo.</summary>
    [Description("SPKI: SubjectPublicKeyInfo")] Spki = 1
}
