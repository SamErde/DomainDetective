namespace DomainDetective;

/// <summary>
/// Robots.txt directive types.
/// </summary>
public enum RobotsDirectiveType
{
    /// <summary>Unrecognized directive.</summary>
    Unknown,
    /// <summary>Allow access to a path.</summary>
    Allow,
    /// <summary>Disallow access to a path.</summary>
    Disallow,
    /// <summary>Crawl delay in seconds.</summary>
    CrawlDelay,
    /// <summary>Sitemap URL.</summary>
    Sitemap,
    /// <summary>Preferred host name.</summary>
    Host
}
