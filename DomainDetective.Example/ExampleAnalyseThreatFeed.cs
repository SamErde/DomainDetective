using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    public static async Task ExampleAnalyseThreatFeed() {
        var healthCheck = new DomainHealthCheck();
        healthCheck.Verbose = false;
        healthCheck.VirusTotalApiKey = "YOUR_API_KEY"; // replace with your key
        healthCheck.AbuseIpDbApiKey = "YOUR_API_KEY"; // replace with your key
        await healthCheck.VerifyThreatFeed("8.8.8.8");
        Helpers.ShowPropertiesTable("Threat feed for 8.8.8.8", healthCheck.ThreatFeedAnalysis);
    }
}
