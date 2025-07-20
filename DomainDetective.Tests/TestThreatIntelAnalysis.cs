using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using DomainDetective;

namespace DomainDetective.Tests;

public class TestThreatIntelAnalysis
{
    [Fact]
    public async Task FlagsListings()
    {
        var vt = new VirusTotalResponse {
            Data = new VirusTotalObject {
                Attributes = new VirusTotalAttributes {
                    LastAnalysisStats = new VirusTotalStats { Malicious = 1 }
                }
            }
        };
        var analysis = new ThreatIntelAnalysis
        {
            GoogleSafeBrowsingOverride = _ => Task.FromResult("{\"matches\":[{}] }"),
            PhishTankOverride = _ => Task.FromResult("{\"results\":{\"valid\":\"true\",\"in_database\":\"true\"}}"),
            VirusTotalOverride = _ => Task.FromResult(JsonSerializer.Serialize(vt, VirusTotalJson.Options))
        };

        await analysis.Analyze("example.com", "g", "p", "v", new InternalLogger());

        Assert.Contains(analysis.Listings, f => f.Source == ThreatIntelSource.GoogleSafeBrowsing && f.IsListed);
        Assert.Contains(analysis.Listings, f => f.Source == ThreatIntelSource.PhishTank && f.IsListed);
        Assert.Contains(analysis.Listings, f => f.Source == ThreatIntelSource.VirusTotal && f.IsListed);
    }

    [Fact]
    public async Task IntegratesWithHealthCheck()
    {
        var health = new DomainHealthCheck();
        health.GoogleSafeBrowsingApiKey = "g";
        health.PhishTankApiKey = "p";
        health.VirusTotalApiKey = "v";
        var vtHealth = new VirusTotalResponse {
            Data = new VirusTotalObject {
                Attributes = new VirusTotalAttributes {
                    LastAnalysisStats = new VirusTotalStats { Malicious = 1 }
                }
            }
        };
        health.ThreatIntelAnalysis.GoogleSafeBrowsingOverride = _ => Task.FromResult("{\"matches\":[{}]}");
        health.ThreatIntelAnalysis.PhishTankOverride = _ => Task.FromResult("{\"results\":{\"valid\":\"true\",\"in_database\":\"true\"}}");
        health.ThreatIntelAnalysis.VirusTotalOverride = _ => Task.FromResult(JsonSerializer.Serialize(vtHealth, VirusTotalJson.Options));

        await health.VerifyThreatIntel("example.com");

        Assert.Contains(health.ThreatIntelAnalysis.Listings, f => f.Source == ThreatIntelSource.GoogleSafeBrowsing && f.IsListed);
        Assert.Contains(health.ThreatIntelAnalysis.Listings, f => f.Source == ThreatIntelSource.PhishTank && f.IsListed);
        Assert.Contains(health.ThreatIntelAnalysis.Listings, f => f.Source == ThreatIntelSource.VirusTotal && f.IsListed);
    }

    [Fact]
    public void ReusesHttpClient()
    {
        var a1 = new ThreatIntelAnalysis();
        var a2 = new ThreatIntelAnalysis();

        Assert.Same(a1.Client, a2.Client);
    }

    [Fact]
    public async Task FeedFailureSetsFailureReason()
    {
        var analysis = new ThreatIntelAnalysis
        {
            GoogleSafeBrowsingOverride = _ => Task.FromException<string>(new HttpRequestException("offline"))
        };

        await analysis.Analyze("example.com", "g", null, null, new InternalLogger());

        Assert.False(string.IsNullOrEmpty(analysis.FailureReason));
        Assert.Contains(analysis.Listings, f => f.Source == ThreatIntelSource.GoogleSafeBrowsing && !f.IsListed);
    }

    [Fact]
    public async Task LogsWarningWhenRiskScoreHigh()
    {
        var logger = new InternalLogger();
        var warnings = new List<LogEventArgs>();
        logger.OnWarningMessage += (_, e) => warnings.Add(e);
        var vtWarn = new VirusTotalResponse {
            Data = new VirusTotalObject {
                Attributes = new VirusTotalAttributes {
                    LastAnalysisStats = new VirusTotalStats { Malicious = 1 },
                    Reputation = 90
                }
            }
        };
        var analysis = new ThreatIntelAnalysis
        {
            VirusTotalOverride = _ => Task.FromResult(JsonSerializer.Serialize(vtWarn, VirusTotalJson.Options))
        };

        await analysis.Analyze("example.com", null, null, "v", logger);

        Assert.Equal(90, analysis.RiskScore);
        Assert.Contains(warnings, w => w.FullMessage.Contains("risk score"));
    }

    [Fact]
    public async Task UsesObjectOverride()
    {
        var obj = new VirusTotalObject
        {
            Attributes = new VirusTotalAttributes
            {
                LastAnalysisStats = new VirusTotalStats { Malicious = 1 },
                Reputation = 42
            }
        };
        var analysis = new ThreatIntelAnalysis
        {
            VirusTotalObjectOverride = _ => Task.FromResult<VirusTotalObject?>(obj)
        };

        await analysis.Analyze("example.com", null, null, "v", new InternalLogger());

        Assert.Contains(analysis.Listings, f => f.Source == ThreatIntelSource.VirusTotal && f.IsListed);
        Assert.Equal(42, analysis.RiskScore);
    }
}
