using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    /// <summary>
    /// Demonstrates running a basic port scan with banner detection.
    /// </summary>
    public static async Task ExamplePortScan()
    {
        var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromSeconds(1) };
        await analysis.Scan("scanme.nmap.org", new[] { 22, 80 }, new InternalLogger());
        Helpers.ShowPropertiesTable("Port Scan Results", analysis.Results);
    }
}
