using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    /// <summary>
    /// Example showing how to retrieve all IPs from an SPF record.
    /// </summary>
    public static async Task ExampleFlattenSpfIps() {
        var healthCheck = new DomainHealthCheck();
        await healthCheck.Verify("github.com", [HealthCheckType.SPF]);
        var analysis = healthCheck.SpfAnalysis.FlattenedIpAnalysis;
        Helpers.ShowPropertiesTable("Flattened SPF IPs for github.com", analysis.UniqueIps);
        Helpers.ShowPropertiesTable("Duplicate SPF IPs for github.com", analysis.DuplicateIps);
    }
}
