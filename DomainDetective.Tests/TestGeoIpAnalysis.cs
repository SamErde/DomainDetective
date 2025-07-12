namespace DomainDetective.Tests;

public class TestGeoIpAnalysis {
    [Fact]
    public void BuiltinLookupReturnsData() {
        var analysis = new GeoIpAnalysis();
        analysis.LoadBuiltinDatabase();
        var info = analysis.Lookup("1.1.1.1");
        Assert.NotNull(info);
        Assert.Equal("AU", info!.Country);
    }

    [Fact]
    public void LookupUnknownReturnsNull() {
        var analysis = new GeoIpAnalysis();
        analysis.LoadBuiltinDatabase();
        var info = analysis.Lookup("192.0.2.1");
        Assert.Null(info);
    }
}
