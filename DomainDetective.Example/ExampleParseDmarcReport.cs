using System.Linq;

namespace DomainDetective.Example;

public static partial class Program {
    /// <summary>
    /// Example parsing a DMARC aggregate report and summarizing failures.
    /// </summary>
    public static void ExampleParseDmarcReport() {
        var report = DmarcReportParser.Parse("aggregate.zip");
        var summaries = report.Records.SummarizeFailuresByIp().ToList();
        Helpers.ShowPropertiesTable("DMARC failures by IP", summaries);
    }
}

