namespace DomainDetective;

/// <summary>
/// Represents an HTTP based IP block list source.
/// </summary>
public class BlockListEntry {
    /// <summary>Gets or sets the list name.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the source URL.</summary>
    public string Url { get; set; }
    /// <summary>Gets or sets whether the list is queried.</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>Gets or sets optional descriptive text.</summary>
    public string Comment { get; set; }
}
