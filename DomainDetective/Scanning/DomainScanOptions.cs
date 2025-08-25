namespace DomainDetective.Scanning;

/// <summary>Scan mode controlling depth and modules.</summary>
public enum ScanMode { Default, Quick, Full }

/// <summary>Options for running a domain scan.</summary>
public sealed class DomainScanOptions
{
    /// <summary>Domain to scan (ASCII or Unicode; will be normalized).</summary>
    public required string Domain { get; init; }

    /// <summary>Scan depth/mode.</summary>
    public ScanMode Mode { get; init; } = ScanMode.Default;

    /// <summary>Enable active mail transport probes (SMTP/IMAP/POP/TLS).</summary>
    public bool ActiveMailProbes { get; init; } = false;

    /// <summary>Max degree of parallelism for stage-internal work.</summary>
    public int MaxDegreeOfParallelism { get; init; } = 6;

    /// <summary>Skip certificate revocation checks for faster HTTPS analysis.</summary>
    public bool SkipRevocation { get; init; } = true;
}

