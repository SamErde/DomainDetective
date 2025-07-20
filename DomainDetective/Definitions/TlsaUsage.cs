namespace DomainDetective;

using System.ComponentModel;

/// <summary>
/// TLSA certificate usage values defined in RFC 6698.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public enum TlsaUsage {
    /// <summary>Usage value not recognized.</summary>
    [Description("Unknown")] Unknown = -1,
    /// <summary>PKIX-TA: CA Constraint.</summary>
    [Description("PKIX-TA: CA Constraint")] PkixTa = 0,
    /// <summary>PKIX-EE: Service Certificate Constraint.</summary>
    [Description("PKIX-EE: Service Certificate Constraint")] PkixEe = 1,
    /// <summary>DANE-TA: Trust Anchor Assertion.</summary>
    [Description("DANE-TA: Trust Anchor Assertion")] DaneTa = 2,
    /// <summary>DANE-EE: Domain Issued Certificate.</summary>
    [Description("DANE-EE: Domain Issued Certificate")] DaneEe = 3
}
