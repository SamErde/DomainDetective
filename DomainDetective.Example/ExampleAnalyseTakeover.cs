using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    /// <summary>
    /// Example demonstrating CNAME takeover risk analysis.
    /// </summary>
    public static async Task ExampleAnalyseTakeover()
    {
        var healthCheck = new DomainHealthCheck { Verbose = false };
        await healthCheck.VerifyTakeoverCname("example.com");
        Helpers.ShowPropertiesTable("Takeover risk for example.com", healthCheck.TakeoverCnameAnalysis);
    }
}
