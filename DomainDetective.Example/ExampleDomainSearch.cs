using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    public static async Task ExampleDomainSearch()
    {
        var search = new DomainAvailabilitySearch
        {
            Prefixes = new[] { "get" },
            Suffixes = new[] { "app" },
            Concurrency = 5,
            TldPreset = "all"
        };

        await foreach (var result in search.SearchAsync(new[] { "example" }))
        {
            var state = result.Available ? "available" : "taken";
            Console.WriteLine($"{result.Domain} - {state}");
        }
    }
}
