namespace DomainDetective;

/// <summary>
/// Well-known DNSSEC digest algorithms.
/// </summary>
public enum DnsDigestType
{
    /// <summary>Unknown or unsupported digest.</summary>
    Unknown = 0,
    /// <summary>SHA-1 digest.</summary>
    Sha1 = 1,
    /// <summary>SHA-256 digest.</summary>
    Sha256 = 2,
    /// <summary>SHA-384 digest.</summary>
    Sha384 = 4,
    /// <summary>SHA-512 digest.</summary>
    Sha512 = 5,
}
