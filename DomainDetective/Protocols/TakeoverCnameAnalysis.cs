using DnsClientX;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective;

/// <summary>
/// Detects CNAME records pointing to cloud providers prone to subdomain takeover.
/// </summary>
/// <para>Part of the DomainDetective project.</para>
public class TakeoverCnameAnalysis
{
    /// <summary>DNS configuration for lookups.</summary>
    public DnsConfiguration DnsConfiguration { get; set; } = new();
    /// <summary>Override DNS query logic.</summary>
    public Func<string, DnsRecordType, Task<DnsAnswer[]>>? QueryDnsOverride { private get; set; }

    /// <summary>Indicates whether a CNAME record exists.</summary>
    public bool CnameRecordExists { get; private set; }
    /// <summary>CNAME target if found.</summary>
    public string? Target { get; private set; }
    /// <summary>True when the CNAME points to a known takeover risk provider.</summary>
    public bool IsTakeoverRisk { get; private set; }

    private static readonly string[] _providerDomains = new[]
    {
        "azurewebsites.net",
        "cloudfront.net",
        "herokudns.com",
        "github.io",
        "amazonaws.com"
    };

    private async Task<DnsAnswer[]> QueryDns(string name, DnsRecordType type)
    {
        if (QueryDnsOverride != null)
        {
            return await QueryDnsOverride(name, type);
        }

        return await DnsConfiguration.QueryDNS(name, type);
    }

    /// <summary>
    /// Queries the domain CNAME and determines if it belongs to a risky provider.
    /// </summary>
    public async Task Analyze(string domainName, InternalLogger logger, CancellationToken ct = default)
    {
        CnameRecordExists = false;
        Target = null;
        IsTakeoverRisk = false;
        ct.ThrowIfCancellationRequested();

        var cname = await QueryDns(domainName, DnsRecordType.CNAME);
        if (cname == null || cname.Length == 0)
        {
            logger?.WriteVerbose("No CNAME record found.");
            return;
        }

        Target = cname[0].Data.TrimEnd('.');
        CnameRecordExists = true;
        logger?.WriteVerbose("CNAME target {0}", Target);

        IsTakeoverRisk = _providerDomains.Any(d => Target.EndsWith(d, StringComparison.OrdinalIgnoreCase));
        if (IsTakeoverRisk)
        {
            logger?.WriteWarning("CNAME target {0} is hosted on a takeover prone provider", Target);
        }
    }
}
