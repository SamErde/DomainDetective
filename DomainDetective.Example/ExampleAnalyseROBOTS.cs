using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    /// <summary>Demonstrates analysis of robots.txt files.</summary>
    public static async Task ExampleAnalyseROBOTS()
    {
        var healthCheck = new DomainHealthCheck { Verbose = false };
        await healthCheck.Verify("example.com", new[] { HealthCheckType.ROBOTS });
        Helpers.ShowPropertiesTable("ROBOTS for example.com", healthCheck.RobotsTxtAnalysis);
        if (healthCheck.RobotsTxtAnalysis.AiBots.Count > 0)
        {
            foreach (var kvp in healthCheck.RobotsTxtAnalysis.AiBots)
            {
                Console.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
            }
        }
    }
}
