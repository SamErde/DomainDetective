using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    /// <summary>
    /// Demonstrates how to check a DNS server for recursion.
    /// </summary>
    public static async Task ExampleAnalyseOpenResolver() {
        var analysis = new OpenResolverAnalysis();
        await analysis.AnalyzeServer("8.8.8.8", 53, new InternalLogger());
        if (analysis.ServerResults.TryGetValue("8.8.8.8:53", out var result)) {
            Console.WriteLine($"Recursion enabled: {result}");
        }
    }
}