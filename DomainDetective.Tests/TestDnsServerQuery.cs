using System.Linq;
using DomainDetective;

namespace DomainDetective.Tests {
    public class TestDnsServerQuery {
        [Fact]
        public void BuilderFiltersByCountry() {
            var analysis = new DnsPropagationAnalysis();
            analysis.LoadBuiltinServers();
            var query = DnsServerQuery.Create().FromCountry("Poland");
            var servers = analysis.FilterServers(query).ToList();
            Assert.NotEmpty(servers);
            CountryIdExtensions.TryParse("Poland", out CountryId pl);
            Assert.All(servers, s => Assert.Equal(pl, s.Country));
        }

        [Fact]
        public void BuilderTakeLimitsResults() {
            var analysis = new DnsPropagationAnalysis();
            analysis.LoadBuiltinServers();
            var query = DnsServerQuery.Create().Take(3);
            var servers = analysis.FilterServers(query).ToList();
            Assert.Equal(3, servers.Count);
        }

        [Fact]
        public void BuilderSupportsMultipleFilters() {
            var analysis = new DnsPropagationAnalysis();
            analysis.LoadBuiltinServers();
            var query = DnsServerQuery.Create().FromCountry("Poland").Take(2);
            var servers = analysis.FilterServers(query).ToList();
            Assert.True(servers.Count <= 2);
            CountryIdExtensions.TryParse("Poland", out CountryId pl);
            Assert.All(servers, s => Assert.Equal(pl, s.Country));
        }

        [Fact]
        public void BuilderIsCaseInsensitive() {
            var analysis = new DnsPropagationAnalysis();
            analysis.LoadBuiltinServers();
            var query = DnsServerQuery.Create().FromCountry("poland");
            var servers = analysis.FilterServers(query).ToList();
            Assert.NotEmpty(servers);
            CountryIdExtensions.TryParse("Poland", out CountryId pl);
            Assert.All(servers, s => Assert.Equal(pl, s.Country));
        }

        [Fact]
        public void BuilderIsCaseInsensitiveForLocation() {
            var analysis = new DnsPropagationAnalysis();
            analysis.LoadBuiltinServers();
            var query = DnsServerQuery.Create().FromLocation("kabul");
            var servers = analysis.FilterServers(query).ToList();
            Assert.NotEmpty(servers);
            LocationIdExtensions.TryParse("Kabul", out var kab);
            Assert.All(servers, s => Assert.Equal(kab, s.Location));
        }

        [Fact]
        public void FilterServersAppliesAllFilters() {
            var analysis = new DnsPropagationAnalysis();
            analysis.LoadBuiltinServers();
            CountryIdExtensions.TryParse("Afghanistan", out var af);
            LocationIdExtensions.TryParse("Kabul", out var kab);
            var query = DnsServerQuery.Create().FilterServers(af, kab, 1);
            var servers = analysis.FilterServers(query).ToList();
            Assert.Single(servers);
            Assert.All(servers, s => {
                Assert.Equal(af, s.Country);
                Assert.Equal(kab, s.Location);
            });
        }
    }
}
