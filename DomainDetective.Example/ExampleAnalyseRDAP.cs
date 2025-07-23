using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    /// <summary>
    /// Example querying RDAP information for a domain.
    /// </summary>
    public static async Task ExampleAnalyseRDAP() {
        var healthCheck = new DomainHealthCheck { Verbose = false };
        healthCheck.RdapAnalysis.CacheDuration = TimeSpan.FromMinutes(10);
        await healthCheck.QueryRDAP("example.com");
        Helpers.ShowPropertiesTable("RDAP for example.com", healthCheck.RdapAnalysis);
    }
}
