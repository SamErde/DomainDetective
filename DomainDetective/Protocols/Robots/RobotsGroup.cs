namespace DomainDetective;

using System.Collections.Generic;

/// <summary>
/// A set of directives applying to specific user agents.
/// </summary>
public sealed class RobotsGroup
{
    /// <summary>User agents the directives apply to.</summary>
    public List<string> UserAgents { get; } = new();
    /// <summary>Directives within the group.</summary>
    public List<RobotsDirective> Directives { get; } = new();
}
