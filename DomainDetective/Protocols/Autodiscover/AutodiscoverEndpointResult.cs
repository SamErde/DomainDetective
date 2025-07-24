namespace DomainDetective;

using System.Collections.Generic;

/// <summary>
/// Result of a single Autodiscover endpoint check.
/// </summary>
public class AutodiscoverEndpointResult {
    /// <summary>Gets the discovery method that produced this result.</summary>
    public AutodiscoverMethod Method { get; init; }
    /// <summary>Gets the URL that was checked.</summary>
    public string? Url { get; init; }
    /// <summary>Gets the HTTP status code returned.</summary>
    public int StatusCode { get; init; }
    /// <summary>Gets the chain of redirects followed, if any.</summary>
    public IReadOnlyList<string>? RedirectChain { get; init; }
    /// <summary>Gets a value indicating whether the XML response was valid.</summary>
    public bool XmlValid { get; init; }
}