using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    /// <summary>
    /// Demonstrates checking a single domain using RDAP.
    /// </summary>
    public static async Task ExampleCheckDomainAvailability()
    {
        var search = new DomainAvailabilitySearch();
        var result = await search.CheckAsync("example.com");
        Console.WriteLine(result.Available
            ? $"{result.Domain} is available"
            : $"{result.Domain} is taken");
    }
}
