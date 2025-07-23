using System;
using System.Threading.Tasks;

namespace DomainDetective.Example
{

public static partial class Program
{
    public static async Task ExampleSearchEngine()
    {
        var analysis = new SearchEngineAnalysis
        {
            GoogleApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
            GoogleCx = Environment.GetEnvironmentVariable("GOOGLE_CX")
        };

        var result = await analysis.SearchGoogle("Domain Detective");
        if (result.Items.Count > 0)
        {
            var item = result.Items[0];
            Console.WriteLine($"First result: {item.Title} - {item.Link}");
            Console.WriteLine($"Snippet: {item.HtmlSnippet}");
            Console.WriteLine($"Total results: {result.Info?.TotalResults}");
        }
    }
}
}
