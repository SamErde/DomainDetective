using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DomainDetective.Tests;

public class TestThreatFeedAnalysis {
    [Fact]
    public async Task FlagsListings() {
        var vt = new VirusTotalResponse {
            Data = new VirusTotalObject {
                Attributes = new VirusTotalAttributes {
                    LastAnalysisStats = new VirusTotalStats { Malicious = 1 }
                }
            }
        };
        var analysis = new ThreatFeedAnalysis {
            VirusTotalOverride = _ => Task.FromResult(JsonSerializer.Serialize(vt, VirusTotalJson.Options)),
            AbuseIpDbOverride = _ => Task.FromResult("{\"data\":{\"abuseConfidenceScore\":20}}")
        };

        await analysis.Analyze("8.8.8.8", "v", "a", new InternalLogger());

        Assert.True(analysis.ListedByVirusTotal);
        Assert.True(analysis.ListedByAbuseIpDb);
    }

    [Fact]
    public async Task IntegratesWithHealthCheck() {
        var health = new DomainHealthCheck();
        health.VirusTotalApiKey = "v";
        health.AbuseIpDbApiKey = "a";
        var vt = new VirusTotalResponse {
            Data = new VirusTotalObject {
                Attributes = new VirusTotalAttributes {
                    LastAnalysisStats = new VirusTotalStats { Malicious = 1 }
                }
            }
        };
        health.ThreatFeedAnalysis.VirusTotalOverride = _ => Task.FromResult(JsonSerializer.Serialize(vt, VirusTotalJson.Options));
        health.ThreatFeedAnalysis.AbuseIpDbOverride = _ => Task.FromResult("{\"data\":{\"abuseConfidenceScore\":20}}");

        await health.VerifyThreatFeed("8.8.8.8");

        Assert.True(health.ThreatFeedAnalysis.ListedByVirusTotal);
        Assert.True(health.ThreatFeedAnalysis.ListedByAbuseIpDb);
    }

    [Fact]
    public void ReusesHttpClient() {
        var a1 = new ThreatFeedAnalysis();
        var a2 = new ThreatFeedAnalysis();

        Assert.Same(a1.Client, a2.Client);
    }

    [Fact]
    public async Task FeedFailureSetsFailureReason() {
        var analysis = new ThreatFeedAnalysis {
            VirusTotalOverride = _ => Task.FromException<string>(new HttpRequestException("offline"))
        };

        await analysis.Analyze("8.8.8.8", "v", null, new InternalLogger());

        Assert.False(string.IsNullOrEmpty(analysis.FailureReason));
        Assert.False(analysis.ListedByVirusTotal);
    }

    [Fact]
    public async Task UsesObjectOverride() {
        var obj = new VirusTotalObject {
            Attributes = new VirusTotalAttributes {
                LastAnalysisStats = new VirusTotalStats { Malicious = 1 }
            }
        };
        var analysis = new ThreatFeedAnalysis {
            VirusTotalObjectOverride = _ => Task.FromResult<VirusTotalObject?>(obj)
        };

        await analysis.Analyze("8.8.8.8", "v", null, new InternalLogger());

        Assert.True(analysis.ListedByVirusTotal);
    }
}
