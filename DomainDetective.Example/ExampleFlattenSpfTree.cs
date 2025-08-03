using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    /// <summary>
    /// Example showing how to render the flattened SPF tree.
    /// </summary>
    public static async Task ExampleFlattenSpfTree() {
        var healthCheck = new DomainHealthCheck();
        await healthCheck.Verify("github.com", [HealthCheckType.SPF]);
        var tree = await healthCheck.SpfAnalysis.GetFlattenedSpfTree();
        Helpers.ShowPropertiesTable("Flattened SPF tree for github.com", tree, unicode: true);
    }
}
