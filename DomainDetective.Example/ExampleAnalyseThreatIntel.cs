using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    public static async Task ExampleAnalyseThreatIntel() {
        var healthCheck = new DomainHealthCheck();
        healthCheck.Verbose = false;
        healthCheck.VirusTotalApiKey = "YOUR_API_KEY"; // replace with your key
        await healthCheck.VerifyThreatIntel("example.com");
        Helpers.ShowPropertiesTable("Threat intel for example.com", healthCheck.ThreatIntelAnalysis);
        Console.WriteLine($"Risk score: {healthCheck.ThreatIntelAnalysis.RiskScore}");
    }
}
