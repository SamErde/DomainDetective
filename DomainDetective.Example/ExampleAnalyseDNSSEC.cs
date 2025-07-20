using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    /// <summary>Demonstrates DNSSEC queries.</summary>
    public static async Task ExampleAnalyseDnsSec() {
        var healthCheck = new DomainHealthCheck();
        healthCheck.Verbose = false;
        await healthCheck.VerifyDNSSEC("example.com");
        DnsSecInfo info = DnsSecConverter.Convert(healthCheck.DnsSecAnalysis);
        Helpers.ShowPropertiesTable("DNSSEC DS Records", info.DsRecords);
    }
}
