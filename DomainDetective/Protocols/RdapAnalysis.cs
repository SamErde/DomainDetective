using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective;

/// <summary>
/// Queries RDAP servers for domain registration data.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public class RdapAnalysis
{
    /// <summary>Domain name returned by the RDAP server.</summary>
    public string DomainName { get; private set; } = string.Empty;
    /// <summary>Registrar display name.</summary>
    public string? Registrar { get; private set; }
    /// <summary>Registrar identifier.</summary>
    public string? RegistrarId { get; private set; }
    /// <summary>Domain creation date string.</summary>
    public string? CreationDate { get; private set; }
    /// <summary>Domain expiration date string.</summary>
    public string? ExpiryDate { get; private set; }
    /// <summary>List of authoritative name servers.</summary>
    public List<string> NameServers { get; private set; } = new();
    /// <summary>Status values reported by RDAP.</summary>
    public List<string> Status { get; private set; } = new();

    internal Func<string, Task<string>>? QueryOverride { get; set; }

    /// <summary>
    /// Retrieves RDAP information for <paramref name="domain"/>.
    /// </summary>
    /// <param name="domain">Domain to query.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task Analyze(string domain, InternalLogger? logger = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new ArgumentNullException(nameof(domain));
        }

        DomainName = domain;
        Registrar = null;
        RegistrarId = null;
        CreationDate = null;
        ExpiryDate = null;
        NameServers = new List<string>();
        Status = new List<string>();

        string json;
        if (QueryOverride != null)
        {
            json = await QueryOverride(domain).ConfigureAwait(false);
        }
        else
        {
            HttpClient client = SharedHttpClient.Instance;
#if NETSTANDARD2_0 || NET472
            using var response = await client.GetAsync($"https://rdap.org/domain/{domain}", cancellationToken).ConfigureAwait(false);
            json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
            json = await client.GetStringAsync($"https://rdap.org/domain/{domain}", cancellationToken).ConfigureAwait(false);
#endif
        }

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("ldhName", out var ldh))
        {
            DomainName = ldh.GetString() ?? DomainName;
        }
        if (doc.RootElement.TryGetProperty("status", out var status))
        {
            foreach (var st in status.EnumerateArray())
            {
                var s = st.GetString();
                if (!string.IsNullOrEmpty(s))
                {
                    Status.Add(s);
                }
            }
        }
        if (doc.RootElement.TryGetProperty("nameservers", out var ns))
        {
            foreach (var n in ns.EnumerateArray())
            {
                if (n.TryGetProperty("ldhName", out var name))
                {
                    var v = name.GetString();
                    if (!string.IsNullOrEmpty(v))
                    {
                        NameServers.Add(v);
                    }
                }
            }
        }
        if (doc.RootElement.TryGetProperty("entities", out var entities))
        {
            foreach (var ent in entities.EnumerateArray())
            {
                if (ent.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        if (string.Equals(role.GetString(), "registrar", StringComparison.OrdinalIgnoreCase))
                        {
                            if (ent.TryGetProperty("handle", out var handle))
                            {
                                RegistrarId = handle.GetString();
                            }
                            if (ent.TryGetProperty("vcardArray", out var vcard) && vcard.ValueKind == JsonValueKind.Array && vcard.GetArrayLength() > 1)
                            {
                                foreach (var card in vcard[1].EnumerateArray())
                                {
                                    if (card.GetArrayLength() > 3 && card[0].GetString() == "fn")
                                    {
                                        Registrar = card[3].GetString();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        if (doc.RootElement.TryGetProperty("events", out var events))
        {
            foreach (var ev in events.EnumerateArray())
            {
                var action = ev.GetProperty("eventAction").GetString();
                var date = ev.GetProperty("eventDate").GetString();
                if (string.Equals(action, "registration", StringComparison.OrdinalIgnoreCase))
                {
                    CreationDate = date;
                }
                else if (string.Equals(action, "expiration", StringComparison.OrdinalIgnoreCase))
                {
                    ExpiryDate = date;
                }
            }
        }
    }
}
