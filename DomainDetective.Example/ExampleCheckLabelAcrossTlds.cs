using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    /// <summary>
    /// Demonstrates checking availability for a label across multiple TLDs.
    /// </summary>
    public static async Task ExampleCheckLabelAcrossTlds()
    {
        var search = new DomainAvailabilitySearch
        {
            Tlds = new[] { "pl", "xyz", "be" }
        };

        await foreach (var result in search.CheckTldsAsync("evotec"))
        {
            var state = result.Available ? "available" : "taken";
            Console.WriteLine($"{result.Domain} - {state}");
        }
    }
}
