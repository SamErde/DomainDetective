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
        const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><feedback><record><identifiers><header_from>bücher.de</header_from></identifiers><row><policy_evaluated><dkim>pass</dkim></policy_evaluated></row></record></feedback>";
        var tmp = Path.GetTempFileName();
        File.Delete(tmp);
        using (var archive = ZipFile.Open(tmp, ZipArchiveMode.Create)) {
            var entry = archive.CreateEntry("report.xml");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(xml);
        }
        var summaries = DmarcReportParser.ParseZip(tmp).ToList();
        File.Delete(tmp);
        Assert.Single(summaries);
        Assert.Equal("xn--bcher-kva.de", summaries[0].Domain);
        Assert.Equal(1, summaries[0].PassCount);
        Assert.Equal(0, summaries[0].FailCount);
    }

    [Fact]
    public void ParseInvalidDomainIsSkipped() {
        const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><feedback><record><identifiers><header_from>in valid</header_from></identifiers><row><policy_evaluated><dkim>pass</dkim></policy_evaluated></row></record></feedback>";
        var tmp = Path.GetTempFileName();
        File.Delete(tmp);
        using (var archive = ZipFile.Open(tmp, ZipArchiveMode.Create)) {
            var entry = archive.CreateEntry("report.xml");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(xml);
        }
        var summaries = DmarcReportParser.ParseZip(tmp).ToList();
        File.Delete(tmp);
        Assert.Empty(summaries);
    }
}