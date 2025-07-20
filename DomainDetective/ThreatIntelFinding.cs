namespace DomainDetective;

/// <summary>
/// Represents the result from a single threat intelligence source.
/// </summary>
public readonly struct ThreatIntelFinding
{
    /// <summary>Origin of the finding.</summary>
    public ThreatIntelSource Source { get; init; }

    /// <summary>Indicates whether the source reported a listing.</summary>
    public bool IsListed { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Source}: {(IsListed ? "Listed" : "Not listed")}";
    }
}
