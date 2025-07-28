using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DomainDetective;

/// <summary>DMARC forensic report details.</summary>
public sealed class DmarcForensicReport {
    /// <summary>IP address of the sending host.</summary>
    public string SourceIp { get; set; } = string.Empty;
    /// <summary>Envelope from address.</summary>
    public string OriginalMailFrom { get; set; } = string.Empty;
    /// <summary>Original recipient address.</summary>
    public string? OriginalRcptTo { get; set; }
    /// <summary>Domain from the From header.</summary>
    public string? HeaderFrom { get; set; }
    /// <summary>Arrival date of the message.</summary>
    public DateTimeOffset? ArrivalDate { get; set; }
}