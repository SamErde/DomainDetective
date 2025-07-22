using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DnsClientX;
using DomainDetective.Helpers;

namespace DomainDetective.Monitoring;

/// <summary>
/// Handles saving and diffing DNS propagation snapshots.
/// </summary>
internal sealed class DnsSnapshotManager
{
    /// <summary>Directory used to store snapshot files.</summary>
    public string? DirectoryPath { get; set; }

    private static string GetPrefix(string domain, DnsRecordType recordType) =>
        $"{domain.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-')}_{recordType}";

    private string? GetSnapshotFile(string domain, DnsRecordType recordType)
    {
        if (string.IsNullOrEmpty(DirectoryPath) || string.IsNullOrEmpty(domain))
        {
            return null;
        }

        Directory.CreateDirectory(DirectoryPath);
        var prefix = GetPrefix(domain, recordType);
        return Path.Combine(DirectoryPath, $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    /// <summary>
    /// Saves DNS results to a snapshot file.
    /// </summary>
    public void SaveSnapshot(string domain, DnsRecordType recordType, IEnumerable<DnsPropagationResult> results)
    {
        var file = GetSnapshotFile(domain, recordType);
        if (file == null || results == null)
        {
            return;
        }

        var json = JsonSerializer.Serialize(results, JsonOptions.Default);
        File.WriteAllText(file, json, Encoding.UTF8);
    }

    /// <summary>
    /// Returns differences between current results and the latest snapshot.
    /// </summary>
    public IEnumerable<string> GetSnapshotChanges(string domain, DnsRecordType recordType, IEnumerable<DnsPropagationResult> results)
    {
        if (string.IsNullOrEmpty(DirectoryPath) || string.IsNullOrEmpty(domain))
        {
            return Array.Empty<string>();
        }

        var prefix = GetPrefix(domain, recordType);
        var files = Directory.GetFiles(DirectoryPath, $"{prefix}_*.json");
        if (files.Length == 0)
        {
            return Array.Empty<string>();
        }

        var previousFile = files.OrderByDescending(f => f).First();
        var previousJson = File.ReadAllText(previousFile);
        var previousResults = JsonSerializer.Deserialize<List<DnsPropagationResult>>(previousJson, JsonOptions.Default) ?? new List<DnsPropagationResult>();

        static string[] ToLines(IEnumerable<DnsPropagationResult> res) => res
            .OrderBy(r => r.Server.IPAddress.ToString())
            .Select(r => $"{r.Server.IPAddress}:{string.Join(",", r.Records ?? Array.Empty<string>())}")
            .ToArray();

        var prevLines = ToLines(previousResults);
        var currLines = ToLines(results);
        var max = Math.Max(prevLines.Length, currLines.Length);
        var changes = new List<string>();
        for (var i = 0; i < max; i++)
        {
            var prev = i < prevLines.Length ? prevLines[i] : string.Empty;
            var curr = i < currLines.Length ? currLines[i] : string.Empty;
            if (!string.Equals(prev, curr, StringComparison.Ordinal))
            {
                changes.Add("- " + prev);
                changes.Add("+ " + curr);
            }
        }
        return changes;
    }
}
