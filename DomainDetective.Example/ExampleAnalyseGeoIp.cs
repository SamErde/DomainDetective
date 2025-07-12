using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    public static async Task ExampleAnalyseGeoIp() {
        var analysis = new GeoIpAnalysis();
        analysis.LoadBuiltinDatabase();
        var info = await analysis.LookupAsync("8.8.8.8");
        Helpers.ShowPropertiesTable("Geo IP for 8.8.8.8", info!);
    }
}
