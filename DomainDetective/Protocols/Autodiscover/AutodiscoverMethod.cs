namespace DomainDetective;

/// <summary>
/// Describes the ordered methods used when looking up Autodiscover endpoints.
/// </summary>
public enum AutodiscoverMethod {
    /// <summary>Lookup using the _autodiscover._tcp SRV record.</summary>
    SrvRecord,
    /// <summary>Use HTTPS on the autodiscover subdomain.</summary>
    AutodiscoverSubdomainHttps,
    /// <summary>Use HTTPS on the root domain.</summary>
    RootDomainHttps,
    /// <summary>Follow HTTP redirect to an alternate host.</summary>
    HttpRedirect
}