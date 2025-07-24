namespace DomainDetective;

/// <summary>
/// Represents a single robots.txt directive.
/// </summary>
public sealed class RobotsDirective
{
    /// <summary>Directive type.</summary>
    public RobotsDirectiveType Type { get; set; }
    /// <summary>Value associated with the directive.</summary>
    public string Value { get; set; } = string.Empty;
}
