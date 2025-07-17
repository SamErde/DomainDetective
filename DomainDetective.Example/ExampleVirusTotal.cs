using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    /// <summary>Demonstrates querying VirusTotal.</summary>
    public static async Task ExampleVirusTotalClient()
    {
        var client = new VirusTotalClient("YOUR_API_KEY");
        var result = await client.GetDomain("example.com");
        if (result?.Data?.Attributes?.LastAnalysisStats is { } stats)
        {
            Helpers.ShowPropertiesTable("VirusTotal stats", stats);
        }
    }
}
