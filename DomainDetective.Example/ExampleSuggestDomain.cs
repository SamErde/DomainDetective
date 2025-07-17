using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    public static async Task ExampleSuggestDomain()
    {
        var search = new DomainAvailabilitySearch();
        await foreach (var result in search.CheckTldAlternativesAsync("example.com"))
        {
            if (result.Available)
            {
                Console.WriteLine($"{result.Domain} is available");
            }
        }
    }
}
