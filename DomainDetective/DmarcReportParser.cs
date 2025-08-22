using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using DomainDetective.Helpers;

namespace DomainDetective;

/// <summary>Parser for zipped DMARC feedback reports.</summary>
public static class DmarcReportParser {
    /// <summary>Parses the specified zip file and returns individual report records.</summary>
    /// <param name="path">Path to the zipped XML feedback report.</param>
    public static IEnumerable<DmarcAggregateRecord> ParseZip(string path) {
        using var archive = ZipFile.OpenRead(path);
        var entry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
        if (entry == null) {
            yield break;
        }

        using var stream = entry.Open();
        XDocument doc = XDocument.Load(stream);
        foreach (var record in doc.Descendants("record")) {
            string rawDomain = record.Element("identifiers")?.Element("header_from")?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(rawDomain)) {
                continue;
            }

            string headerFrom;
            try {
                headerFrom = DomainHelper.ValidateIdn(rawDomain);
            } catch (ArgumentException) {
                continue;
            }

            string sourceIp = record.Element("row")?.Element("source_ip")?.Value ?? string.Empty;
            string dkim = record.Element("row")?.Element("policy_evaluated")?.Element("dkim")?.Value ?? string.Empty;
            string spf = record.Element("row")?.Element("policy_evaluated")?.Element("spf")?.Value ?? string.Empty;
            string disposition = record.Element("row")?.Element("policy_evaluated")?.Element("disposition")?.Value ?? string.Empty;
            string countStr = record.Element("row")?.Element("count")?.Value ?? "1";
            if (!int.TryParse(countStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int count)) {
                count = 1;
            }

            yield return new DmarcAggregateRecord {
                SourceIp = sourceIp,
                HeaderFrom = headerFrom,
                Count = count,
                Dkim = dkim,
                Spf = spf,
                Disposition = disposition
            };
        }
    }

    /// <summary>Parses multiple zipped DMARC reports and returns individual report records.</summary>
    /// <param name="paths">Paths to zipped XML feedback reports.</param>
    public static IEnumerable<DmarcAggregateRecord> ParseMultiple(IEnumerable<string> paths) {
        foreach (var path in paths) {
            foreach (var record in ParseZip(path)) {
                yield return record;
            }
        }
    }
}

