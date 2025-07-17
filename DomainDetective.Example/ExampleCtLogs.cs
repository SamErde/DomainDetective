using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    /// <summary>Demonstrates retrieving CT log entries for a certificate.</summary>
    public static async Task ExampleCtLogAggregator()
    {
        var analysis = await CertificateAnalysis.CheckWebsiteCertificate("https://google.com");
        Helpers.ShowPropertiesTable("CT log entries", analysis.CtLogEntries);
    }
}
