using System.Threading.Tasks;

namespace DomainDetective.Example;

/// <summary>
/// Demonstrates Autodiscover analysis.
/// </summary>
public static partial class Program {
    /// <summary>Runs the Autodiscover example.</summary>
    public static async Task ExampleAnalyseAutodiscover() {
        var healthCheck = new DomainHealthCheck();
        healthCheck.Verbose = false;
        await healthCheck.VerifyAutodiscover("example.com");
        Helpers.ShowPropertiesTable("Autodiscover DNS", healthCheck.AutodiscoverAnalysis);
        Helpers.ShowPropertiesTable("Autodiscover Endpoints", healthCheck.AutodiscoverHttpAnalysis.Endpoints);
    }
}
