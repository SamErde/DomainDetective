using System;
using System.Linq;
using Xunit;

namespace DomainDetective.Tests;

public class TestPropertyNaming
{
    [Fact]
    public void DomainHealthCheckHasUniquePropertyNamesIgnoringCase()
    {
        var properties = typeof(DomainHealthCheck).GetProperties();
        var duplicates = properties
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.Empty(duplicates);
    }
}
