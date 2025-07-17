using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective;

/// <summary>
/// Collection of predefined TLD presets.
/// </summary>
public static class DomainAvailabilityPresets
{
    /// <summary>Mapping of preset names to TLD arrays.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> TldPresets;

    /// <summary>Complete list of TLDs from the embedded public suffix list.</summary>
    public static readonly string[] All;

    static DomainAvailabilityPresets()
    {
        All = LoadAllTlds();
        TldPresets = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["common"] = new[] { "com", "net", "org", "co", "io", "app", "dev" },
            ["tech"] = new[] { "dev", "io", "app", "ai", "tech" },
            ["fun"] = new[] { "lol", "fun", "xyz", "site" },
            ["all"] = All
        };
    }

    private static string[] LoadAllTlds()
    {
        using var stream = typeof(DomainAvailabilityPresets).Assembly
            .GetManifestResourceStream("DomainDetective.public_suffix_list.dat");
        if (stream == null)
        {
            return Array.Empty<string>();
        }

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//"))
            {
                continue;
            }

            if (trimmed.StartsWith("!"))
            {
                trimmed = trimmed.Substring(1);
            }

            if (trimmed.StartsWith("*."))
            {
                trimmed = trimmed.Substring(2);
            }

            var idx = trimmed.LastIndexOf('.');
            var tld = idx >= 0 ? trimmed.Substring(idx + 1) : trimmed;
            set.Add(tld);
        }

        return set.ToArray();
    }
}

/// <summary>
/// Result of a single domain availability check.
/// </summary>
public sealed record DomainAvailabilityResult(string Domain, bool Available);

/// <summary>
/// Generates domain permutations and checks availability using RDAP.
/// </summary>
public class DomainAvailabilitySearch
{
    /// <summary>Prefixes used to generate permutations.</summary>
    public IReadOnlyList<string> Prefixes { get; set; } = Array.Empty<string>();

    /// <summary>Suffixes used to generate permutations.</summary>
    public IReadOnlyList<string> Suffixes { get; set; } = Array.Empty<string>();

    /// <summary>TLDs used to generate permutations.</summary>
    public IReadOnlyList<string> Tlds { get; set; } = new[] { "com" };

    /// <summary>Name of the active TLD preset.</summary>
    public string? TldPreset
    {
        get => _preset;
        set
        {
            _preset = value;
            if (!string.IsNullOrWhiteSpace(value) && DomainAvailabilityPresets.TldPresets.TryGetValue(value!, out var preset))
            {
                Tlds = preset;
            }
        }
    }

    private string? _preset;

    /// <summary>Minimum length for the domain label.</summary>
    public int MinLength { get; set; }

    /// <summary>Maximum length for the domain label.</summary>
    public int MaxLength { get; set; } = int.MaxValue;

    /// <summary>Maximum concurrent RDAP queries.</summary>
    public int Concurrency { get; set; } = 10;

    /// <summary>Override to simulate RDAP queries for testing.</summary>
    internal Func<string, CancellationToken, Task<bool>>? AvailabilityOverride { private get; set; }
        

    /// <summary>
    /// Generates domain permutations for specified keywords.
    /// </summary>
    /// <param name="keywords">Keywords used to build domain names.</param>
    /// <returns>Enumeration of domain names.</returns>
    public IEnumerable<string> Generate(IEnumerable<string> keywords)
    {
        foreach (var keyword in keywords.Where(k => !string.IsNullOrWhiteSpace(k)))
        {
            var key = keyword.Trim().ToLowerInvariant();
            foreach (var tld in Tlds)
            {
                foreach (var domain in EnumerateForKeyword(key, tld))
                {
                    yield return domain;
                }
            }
        }
    }

    private IEnumerable<string> EnumerateForKeyword(string keyword, string tld)
    {
        var baseLabel = keyword;
        yield return $"{baseLabel}.{tld}";

        foreach (var prefix in Prefixes)
        {
            yield return $"{prefix}{baseLabel}.{tld}";
        }

        foreach (var suffix in Suffixes)
        {
            yield return $"{baseLabel}{suffix}.{tld}";
        }

        foreach (var prefix in Prefixes)
        {
            foreach (var suffix in Suffixes)
            {
                yield return $"{prefix}{baseLabel}{suffix}.{tld}";
            }
        }
    }

    private static readonly RdapClient _rdapClient = new();

    private async Task<bool> IsAvailableAsync(string domain, CancellationToken ct)
    {
        if (AvailabilityOverride != null)
        {
            return await AvailabilityOverride(domain, ct).ConfigureAwait(false);
        }

        try
        {
            var result = await _rdapClient.GetDomain(domain, ct).ConfigureAwait(false);
            return result == null;
        }
        catch (HttpRequestException ex)
        {
#if NET6_0_OR_GREATER
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return true;
            }
            throw;
#else
            if (ex.Message.Contains("404"))
            {
                return true;
            }
            throw;
#endif
        }
    }

    /// <summary>
    /// Asynchronously checks availability for generated domain names.
    /// </summary>
    /// <param name="keywords">Keywords used to generate domains.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Stream of availability results.</returns>
    public async IAsyncEnumerable<DomainAvailabilityResult> SearchAsync(
        IEnumerable<string> keywords,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var domains = new Queue<string>(Generate(keywords)
            .Where(d =>
            {
                var idx = d.LastIndexOf('.');
                var label = idx > 0 ? d.Substring(0, idx) : d;
                return label.Length >= MinLength && label.Length <= MaxLength;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase));

        var tasks = new List<Task<DomainAvailabilityResult>>();

        async Task<DomainAvailabilityResult> Check(string dom)
        {
            var available = await IsAvailableAsync(dom, ct).ConfigureAwait(false);
            return new DomainAvailabilityResult(dom, available);
        }

        for (var i = 0; i < Concurrency && domains.Count > 0; i++)
        {
            tasks.Add(Check(domains.Dequeue()));
        }

        while (tasks.Count > 0)
        {
            var finished = await Task.WhenAny(tasks).ConfigureAwait(false);
            tasks.Remove(finished);
            yield return await finished.ConfigureAwait(false);
            if (domains.Count > 0)
            {
                tasks.Add(Check(domains.Dequeue()));
            }
        }
    }
}
