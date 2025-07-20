namespace DomainDetective;

/// <summary>
/// Identifies issues detected during message header analysis.
/// </summary>
public enum MessageHeaderIssue {
    /// <summary>No ARC headers were found.</summary>
    MissingArc,
    /// <summary>The ARC chain exists but failed validation.</summary>
    InvalidArc,
    /// <summary>One or more DKIM signatures were invalid.</summary>
    InvalidDkim
}
