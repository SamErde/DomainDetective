using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective;

/// <summary>
/// Provides geolocation information using a local IP database.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public sealed class GeoIpAnalysis {
    private readonly Dictionary<string, GeoLocationInfo> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Loads entries from a CSV file.</summary>
    /// <param name="filePath">Path to the CSV file.</param>
    /// <param name="clearExisting">When true the current database is cleared.</param>
    public void LoadDatabase(string filePath, bool clearExisting = true) {
        if (string.IsNullOrWhiteSpace(filePath)) {
            throw new ArgumentNullException(nameof(filePath));
        }
        using var stream = File.OpenRead(filePath);
        LoadFromStream(stream, clearExisting);
    }

    /// <summary>Loads entries from the embedded database.</summary>
    /// <param name="clearExisting">When true the current database is cleared.</param>
    public void LoadBuiltinDatabase(bool clearExisting = true) {
        using var stream = typeof(GeoIpAnalysis).Assembly.GetManifestResourceStream("DomainDetective.geoip.csv");
        if (stream != null) {
            LoadFromStream(stream, clearExisting);
        }
    }

    private void LoadFromStream(Stream stream, bool clearExisting) {
        if (clearExisting) {
            _entries.Clear();
        }
        using var reader = new StreamReader(stream);
        string? line;
        bool first = true;
        while ((line = reader.ReadLine()) != null) {
            if (first) {
                first = false;
                continue;
            }
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }
            var parts = line.Split(new[] { ',' }, 3);
            if (parts.Length < 3) {
                continue;
            }
            var ip = parts[0].Trim();
            var country = parts[1].Trim();
            var region = parts[2].Trim();
            _entries[ip] = new GeoLocationInfo { Country = country, Region = region };
        }
    }

    /// <summary>Looks up geolocation information for an IP address.</summary>
    /// <param name="ip">IP address to query.</param>
    /// <returns>Geolocation info or null if not found.</returns>
    public GeoLocationInfo? Lookup(string ip) => _entries.TryGetValue(ip, out var info) ? info : null;

    /// <summary>Asynchronously looks up geolocation information.</summary>
    /// <param name="ip">IP address to query.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<GeoLocationInfo?> LookupAsync(string ip, CancellationToken ct = default) => Task.FromResult(Lookup(ip));
}
