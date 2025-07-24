namespace DomainDetective;

using System.Collections.Generic;

/// <summary>
/// Represents a robots.txt file.
/// </summary>
public sealed class RobotsFile
{
    /// <summary>Directive groups.</summary>
    public List<RobotsGroup> Groups { get; } = new();

    /// <summary>Declared sitemap URLs.</summary>
    public List<string> Sitemaps { get; } = new();

    /// <summary>Preferred host name.</summary>
    public string? Host { get; set; }
}
