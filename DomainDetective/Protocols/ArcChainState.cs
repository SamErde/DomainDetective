namespace DomainDetective;

/// <summary>
/// Describes the status of an ARC chain.
/// </summary>
public enum ArcChainState
{
    /// <summary>No ARC headers were present.</summary>
    Missing,
    /// <summary>ARC headers were found but the chain is invalid.</summary>
    Invalid,
    /// <summary>The ARC chain is valid.</summary>
    Valid
}
