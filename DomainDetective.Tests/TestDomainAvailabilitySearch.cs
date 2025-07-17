using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainDetective.Tests;

public class TestDomainAvailabilitySearch
{
    [Fact]
    public void GeneratesPermutations()
    {
        var search = new DomainAvailabilitySearch
        {
            Prefixes = new[] { "my" },
            Suffixes = new[] { "app" },
            Tlds = new[] { "com" }
        };

        var domains = search.Generate(new[] { "test" }).ToArray();

        Assert.Contains("test.com", domains);
        Assert.Contains("mytest.com", domains);
        Assert.Contains("testapp.com", domains);
        Assert.Contains("mytestapp.com", domains);
    }

    [Fact]
    public async Task FiltersByLengthAndUsesOverride()
    {
        var search = new DomainAvailabilitySearch
        {
            Tlds = new[] { "com" },
            MinLength = 3,
            MaxLength = 10,
            AvailabilityOverride = (d, _) => Task.FromResult(!d.StartsWith("taken"))
        };

        var results = new List<DomainAvailabilityResult>();
        await foreach (var result in search.SearchAsync(new[] { "free", "taken" }))
        {
            results.Add(result);
        }

        Assert.Contains(results, r => r.Domain == "free.com" && r.Available);
        Assert.Contains(results, r => r.Domain == "taken.com" && !r.Available);
    }

    [Fact]
    public void AppliesPreset()
    {
        var search = new DomainAvailabilitySearch
        {
            TldPreset = "tech"
        };

        Assert.Contains("io", search.Tlds);
        Assert.Contains("dev", search.Tlds);
    }

    [Fact]
    public void AppliesAllPreset()
    {
        var search = new DomainAvailabilitySearch
        {
            TldPreset = "all"
        };

        Assert.Contains("com", search.Tlds);
        Assert.True(search.Tlds.Count > 100);
    }

    [Fact]
    public async Task CheckAsyncReturnsExpectedResult()
    {
        var search = new DomainAvailabilitySearch
        {
            AvailabilityOverride = (d, _) => Task.FromResult(d == "free.com")
        };

        var result = await search.CheckAsync("free.com");
        Assert.True(result.Available);
        Assert.Equal("free.com", result.Domain);
    }

    [Fact]
    public async Task CheckTldsAsyncEnumeratesAcrossTlds()
    {
        var search = new DomainAvailabilitySearch
        {
            Tlds = new[] { "com", "net" },
            AvailabilityOverride = (d, _) => Task.FromResult(d.EndsWith(".com"))
        };

        var results = new List<DomainAvailabilityResult>();
        await foreach (var r in search.CheckTldsAsync("example"))
        {
            results.Add(r);
        }

        Assert.Contains(results, r => r.Domain == "example.com" && r.Available);
        Assert.Contains(results, r => r.Domain == "example.net" && !r.Available);
    }

}
