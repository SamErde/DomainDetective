using DomainDetective;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xunit;

namespace DomainDetective.Tests;

public class TestDmarcReportParser {
    [Fact]
    public void ParseUnicodeDomain() {
        const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><feedback><record><identifiers><header_from>bücher.de</header_from></identifiers><row><source_ip>1.2.3.4</source_ip><policy_evaluated><dkim>pass</dkim></policy_evaluated></row></record></feedback>";
        var tmp = Path.GetTempFileName();
        File.Delete(tmp);
        using (var archive = ZipFile.Open(tmp, ZipArchiveMode.Create)) {
            var entry = archive.CreateEntry("report.xml");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(xml);
        }
        var records = DmarcReportParser.ParseZip(tmp).ToList();
        File.Delete(tmp);
        Assert.Single(records);
        Assert.Equal("xn--bcher-kva.de", records[0].HeaderFrom);
        Assert.Equal("1.2.3.4", records[0].SourceIp);
    }

    [Fact]
    public void SummarizeFailures() {
        const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><feedback>" +
            "<record><identifiers><header_from>example.com</header_from></identifiers><row><source_ip>1.2.3.4</source_ip><count>2</count><policy_evaluated><dkim>fail</dkim><spf>fail</spf><disposition>reject</disposition></policy_evaluated></row></record>" +
            "<record><identifiers><header_from>example.org</header_from></identifiers><row><source_ip>5.6.7.8</source_ip><count>1</count><policy_evaluated><dkim>pass</dkim><spf>fail</spf><disposition>none</disposition></policy_evaluated></row></record>" +
            "</feedback>";
        var tmp = Path.GetTempFileName();
        File.Delete(tmp);
        using (var archive = ZipFile.Open(tmp, ZipArchiveMode.Create)) {
            var entry = archive.CreateEntry("report.xml");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(xml);
        }
        var records = DmarcReportParser.ParseZip(tmp).ToList();
        File.Delete(tmp);
        var failures = records.GetFailureRecords().ToList();
        Assert.Single(failures);
        Assert.Equal("1.2.3.4", failures[0].SourceIp);
        Assert.Equal(2, failures[0].Count);

        var byIp = records.SummarizeFailuresByIp().Single();
        Assert.Equal("1.2.3.4", byIp.SourceIp);
        Assert.Equal(2, byIp.Count);

        var byHeaderFrom = records.SummarizeFailuresByHeaderFrom().Single();
        Assert.Equal("example.com", byHeaderFrom.HeaderFrom);
        Assert.Equal(2, byHeaderFrom.Count);
    }
}

