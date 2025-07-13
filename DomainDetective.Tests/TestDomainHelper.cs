using DomainDetective.Helpers;
using System;

namespace DomainDetective.Tests;

public class TestDomainHelper
{
    [Fact]
    public void ValidateIdnConvertsUnicode()
    {
        var result = DomainHelper.ValidateIdn("bücher.de");
        Assert.Equal("xn--bcher-kva.de", result);
    }

    [Fact]
    public void ValidateIdnTrimsWhitespace()
    {
        var result = DomainHelper.ValidateIdn("  bücher.de.  ");
        Assert.Equal("xn--bcher-kva.de", result);
    }

    [Fact]
    public void ValidateIdnNullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => DomainHelper.ValidateIdn(null!));
    }
}
