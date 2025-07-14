using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DomainDetective;

/// <summary>
/// Loads and evaluates IP block lists from text sources.
/// </summary>
public class IpBlockListAnalysis {
    private readonly Dictionary<string, List<IpCidrRange>> _lists = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Configured block list entries.</summary>
    public List<BlockListEntry> Entries { get; } = new();

    /// <summary>Gets the names of lists containing the address.</summary>
    public IEnumerable<string> ListsContaining(IPAddress address) {
        foreach (var pair in _lists) {
            if (pair.Value.Any(r => r.Contains(address))) {
                yield return pair.Key;
            }
        }
    }

    /// <summary>Adds ranges from plain text.</summary>
    public void LoadFromString(string name, string content, bool clearExisting = true) {
        if (clearExisting || !_lists.TryGetValue(name, out var ranges)) {
            ranges = new List<IpCidrRange>();
            _lists[name] = ranges;
        }
        foreach (var line in content.Split('\n')) {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";"))
                continue;
            var token = trimmed.Split(new[] { ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries)[0];
            if (IpCidrRange.TryParse(token, out var range)) {
                ranges.Add(range);
            }
        }
    }

    /// <summary>Downloads and parses all enabled lists.</summary>
    public async Task UpdateAsync(bool overwriteExisting = true, HttpClient? client = null) {
        client ??= SharedHttpClient.Instance;
        foreach (var entry in Entries.Where(e => e.Enabled && !string.IsNullOrEmpty(e.Url))) {
            var content = await client.GetStringAsync(entry.Url);
            LoadFromString(entry.Name, content, overwriteExisting);
        }
    }
}
