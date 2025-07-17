using System;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program {
    /// <summary>Demonstrates querying an NTP server.</summary>
    public static async Task ExampleAnalyseNtp() {
        var analysis = new NtpAnalysis();
        await analysis.AnalyzeServer("pool.ntp.org", 123, new InternalLogger());
        if (analysis.ServerResults.TryGetValue("pool.ntp.org:123", out var result)) {
            Helpers.ShowPropertiesTable("NTP", result);
        }
    }
}
