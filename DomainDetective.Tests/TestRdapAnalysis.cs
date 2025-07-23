using System;
using System.Threading.Tasks;

namespace DomainDetective.Tests {
    public class TestRdapAnalysis {
        [Fact]
        public async Task ParsesRdapResponse() {
            const string json = "{\"ldhName\":\"example.com\",\"status\":[\"active\"],\"nameservers\":[{\"ldhName\":\"ns1.example.net\"},{\"ldhName\":\"ns2.example.net\"}],\"events\":[{\"eventAction\":\"registration\",\"eventDate\":\"2000-01-01T00:00:00Z\"},{\"eventAction\":\"expiration\",\"eventDate\":\"2030-01-01T00:00:00Z\"}],\"entities\":[{\"handle\":\"123\",\"roles\":[\"registrar\"],\"vcardArray\":[\"vcard\",[[\"fn\",{},\"text\",\"Registrar Inc\"]]]}]}";
            var analysis = new RdapAnalysis { QueryOverride = _ => Task.FromResult(json) };
            await analysis.Analyze("example.com", new InternalLogger());
            Assert.Equal("example.com", analysis.DomainName.ToLowerInvariant());
            Assert.Equal("Registrar Inc", analysis.Registrar);
            Assert.Equal("123", analysis.RegistrarId);
            Assert.Equal("2000-01-01T00:00:00Z", analysis.CreationDate);
            Assert.Equal("2030-01-01T00:00:00Z", analysis.ExpiryDate);
            Assert.Contains("ns1.example.net", analysis.NameServers);
            Assert.Contains("ns2.example.net", analysis.NameServers);
            Assert.Contains(RdapDomainStatus.Active, analysis.Status);
        }

        [Fact]
        public async Task IntegratesWithHealthCheck() {
            const string json = "{\"ldhName\":\"example.org\"}";
            var health = new DomainHealthCheck();
            health.RdapAnalysis.QueryOverride = _ => Task.FromResult(json);
            await health.QueryRDAP("example.org");
            Assert.Equal("example.org", health.RdapAnalysis.DomainName.ToLowerInvariant());
        }

        [Fact]
        public async Task CachedResultReusedUntilExpiration() {
            RdapAnalysis.ClearCache();
            int hitCount = 0;
            string json = "{\"ldhName\":\"example.net\"}";
            var analysis = new RdapAnalysis {
                CacheDuration = TimeSpan.FromMilliseconds(500),
                QueryOverride = _ => { hitCount++; return Task.FromResult(json); }
            };

            await analysis.Analyze("example.net", new InternalLogger());
            await analysis.Analyze("example.net", new InternalLogger());

            Assert.Equal(1, hitCount);

            await Task.Delay(600);
            var analysis2 = new RdapAnalysis {
                CacheDuration = TimeSpan.FromMilliseconds(500),
                QueryOverride = _ => { hitCount++; return Task.FromResult(json); }
            };
            await analysis2.Analyze("example.net", new InternalLogger());

            Assert.Equal(2, hitCount);
        }
    }
}
