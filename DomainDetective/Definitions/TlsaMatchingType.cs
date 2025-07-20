namespace DomainDetective;

using System.ComponentModel;

/// <summary>
/// TLSA matching type values defined in RFC 6698.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public enum TlsaMatchingType {
    /// <summary>Matching type not recognized.</summary>
    [Description("Unknown")] Unknown = -1,
    /// <summary>Full: Full Certificate or SPKI.</summary>
    [Description("Full: Full Certificate or SPKI")] Full = 0,
    /// <summary>SHA-256: SHA-256 of Certificate or SPKI.</summary>
    [Description("SHA-256: SHA-256 of Certificate or SPKI")] Sha256 = 1,
    /// <summary>SHA-512: SHA-512 of Certificate or SPKI.</summary>
    [Description("SHA-512: SHA-512 of Certificate or SPKI")] Sha512 = 2
}
