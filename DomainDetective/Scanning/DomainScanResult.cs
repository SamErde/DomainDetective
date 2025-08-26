using System;
using System.Collections.Generic;

namespace DomainDetective.Scanning;

/// <summary>Aggregate result for a domain scan.</summary>
public sealed class DomainScanResult
{
    public string Domain { get; init; } = string.Empty;
    public DateTime StartedUtc { get; init; } = DateTime.UtcNow;
    public DateTime FinishedUtc { get; set; }

    public DnsResult Dns { get; init; } = new();
    public MailResult Mail { get; init; } = new();
    public WebResult Web { get; init; } = new();
    public ReputationResult Reputation { get; init; } = new();

    public List<string> Notes { get; } = new();
}

/// <summary>DNS-related results.</summary>
public sealed class DnsResult
{
    public SoaInfo? Soa { get; set; }
    public List<string> Ns { get; set; } = new();
    public List<MxInfo> Mx { get; set; } = new();
    public bool? DnssecEnabled { get; set; }
    public bool? ZoneTransferOpen { get; set; }
    public bool? WildcardDetected { get; set; }
    public bool? OpenResolver { get; set; }
    public TimeSpan? MinTtl { get; set; }
    public TimeSpan? MaxTtl { get; set; }
    public List<PtrInfo> Reverse { get; set; } = new();
    public bool? FCrDnsAligned { get; set; }
}

/// <summary>Mail-related results.</summary>
public sealed class MailResult
{
    public string? SpfRecord { get; set; }
    public string? DmarcRecord { get; set; }
    public string? DkimSelectorHint { get; set; }
    public Dictionary<string, bool?> DkimSelectorsOk { get; set; } = new();
    public string? BimiRecord { get; set; }
    public string? MtaStsPolicy { get; set; }
    public string? TlsRpt { get; set; }

    public bool? SmtpStartTlsOk { get; set; }
    public bool? ImapTlsOk { get; set; }
    public bool? Pop3TlsOk { get; set; }
    public bool? OpenRelaySuspected { get; set; }

    public MailPolicyScore PolicyScore { get; set; } = new();
}

/// <summary>Web-related results.</summary>
public sealed class WebResult
{
    public bool? HttpOk { get; set; }
    public bool? HttpsOk { get; set; }
    public bool? Http2 { get; set; }
    public bool? Http3 { get; set; }
    public bool? Hsts { get; set; }
    public List<string> SecurityHeadersMissing { get; set; } = new();
    public TlsChainInfo? Tls { get; set; }
    public bool? DaneTlsa { get; set; }
    public bool? Smimea { get; set; }
}

/// <summary>Reputation and registry-related results.</summary>
public sealed class ReputationResult
{
    public string? WhoisRegistrar { get; set; }
    public string? RdapHandle { get; set; }
    public bool? RpkiValid { get; set; }
    public List<string> Blacklists { get; set; } = new();
}

// --- Submodels ---
public sealed class SoaInfo { public string? PrimaryNs { get; set; } public string? RName { get; set; } public long? Serial { get; set; } }
public sealed class MxInfo { public string Host { get; set; } = string.Empty; public int Preference { get; set; } public bool? Resolvable { get; set; } }
public sealed class PtrInfo { public string Ip { get; set; } = string.Empty; public string? Ptr { get; set; } }
public sealed class TlsChainInfo { public string? Subject { get; set; } public string? Issuer { get; set; } public DateTimeOffset? NotAfter { get; set; } }

/// <summary>Simple posture scoring for wizard visualizations.</summary>
public sealed class MailPolicyScore
{
    public int SpfCoverage { get; set; }
    public int DkimSelectors { get; set; }
    public int DmarcStrength { get; set; }
    public int TransportTlsPosture { get; set; }
}

